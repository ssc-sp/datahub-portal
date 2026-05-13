using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Datahub.Infrastructure.Services.Storage;

public class DataLakeClientService
{
    private const string DatahubSecretName = "Datahub-StorageDL-Secret";
    private ILogger<DataLakeClientService> _logger;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IUserInformationService _userInformationService;
    private IOptions<APITargets> _targets;
    private TokenCredential _tokenCredential;
    private Dictionary<string, DataLakeServiceClient> _projectServiceClients;
    private BlobServiceClient _blobServiceClient;

    public DataLakeClientService(ILogger<DataLakeClientService> logger,
        ITokenAcquisition tokenAcquisition,
        IUserInformationService userInformationService,
        IOptions<APITargets> targets
    )
    {
        _logger = logger;
        _tokenAcquisition = tokenAcquisition;
        _userInformationService = userInformationService;
        _targets = targets;
    }

    private DataLakeServiceClient dataLakeServiceClient { get; set; }
    private DataLakeFileSystemClient dataLakeFileSystemClient { get; set; }
    
    private async Task SetDataLakeServiceClient()
    {
        await EnsureTokenCredentialAsync();

        string dfsUri = $"https://{_targets.Value.StorageAccountName}.dfs.core.windows.net";
        dataLakeServiceClient = new DataLakeServiceClient(new Uri(dfsUri), _tokenCredential);
        dataLakeFileSystemClient = dataLakeServiceClient.GetFileSystemClient(_targets.Value.FileSystemName);
        
        // Also initialize BlobServiceClient for User Delegation Keys
        string blobUri = $"https://{_targets.Value.StorageAccountName}.blob.core.windows.net";
        _blobServiceClient = new BlobServiceClient(new Uri(blobUri), _tokenCredential);
    }

    private async Task EnsureTokenCredentialAsync()
    {
        if (_tokenCredential is not null)
        {
            return;
        }

        await GetStorageAccessTokenAsync();
        _tokenCredential = new DelegatedUserTokenCredential(GetStorageAccessTokenAsync);
    }

    private async Task<string> GetStorageAccessTokenAsync()
    {
        var user = await _userInformationService.GetAuthenticatedUser();
        var scopes = new[] { "https://storage.azure.com/user_impersonation" };

        return await _tokenAcquisition.GetAccessTokenForUserAsync(
            scopes,
            authenticationScheme: OpenIdConnectDefaults.AuthenticationScheme,
            user: user);
    }

    private sealed class DelegatedUserTokenCredential(Func<Task<string>> tokenFactory) : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var token = tokenFactory().GetAwaiter().GetResult();
            return new AccessToken(token, DateTimeOffset.UtcNow.AddMinutes(50));
        }

        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var token = await tokenFactory();
            return new AccessToken(token, DateTimeOffset.UtcNow.AddMinutes(50));
        }
    }

    /// <summary>
    /// Gets TokenCredential for User Delegation SAS (recommended approach)
    /// </summary>
    public TokenCredential GetTokenCredential()
    {
        return _tokenCredential;
    }

    /// <summary>
    /// Gets initialized TokenCredential for User Delegation SAS using logged-in user token.
    /// </summary>
    public async Task<TokenCredential> GetTokenCredentialAsync()
    {
        await EnsureTokenCredentialAsync();
        return _tokenCredential;
    }

    /// <summary>
    /// Gets BlobServiceClient for User Delegation SAS generation
    /// </summary>
    public async Task<BlobServiceClient> GetBlobServiceClient()
    {
        await CheckClients();
        return _blobServiceClient;
    }

    public async Task<DataLakeServiceClient> GetDataLakeServiceClient()
    {
        await CheckClients();
        return dataLakeServiceClient;
    }

    public async Task<DataLakeFileSystemClient> GetDataLakeFileSystemClient()
    {
        await CheckClients();
        return dataLakeFileSystemClient;            
    }

    public async Task<bool> AssignOwnerPermissionsToFile(FileMetaData file, string userId, string permissions)
    {
        var accessControlTuple = await GetAccessControlList(file);
        var accessControlList = accessControlTuple.Item1;
        var fileClient = accessControlTuple.Item2;

        IList<PathAccessControlItem> listItem = PathAccessControlExtensions.ParseAccessControlList($"user:{userId}:{permissions}"); //rwx

        // 1) Check to see if user is already in the list
        var newPerm = listItem.First();
        var userPerm = accessControlList.FirstOrDefault(p => p.EntityId == newPerm.EntityId);
        if (userPerm != null)
        {
            userPerm.Permissions = newPerm.Permissions;
        }
        else
        {
            accessControlList.Add(newPerm);
        }
        var response = fileClient.SetAccessControlList(accessControlList);
        if (response.GetRawResponse().Status == 200)
        {
            await LoadSharedUsers(file);

            file.lastmodifiedts = DateTime.UtcNow;
            fileClient.SetMetadata(file.GenerateMetadata());
            //await _cognitiveSearchService.EditDocument(file);

            return true;
        }

        return false;
    }

    public async Task<bool> RemoveSharedUser(FileMetaData file, string user)
    {
        var accessControlTuple = await GetAccessControlList(file);
        var accessControlList = accessControlTuple.Item1;
        var fileClient = accessControlTuple.Item2;
        var item = accessControlList.Where(a => a.EntityId == user).FirstOrDefault();
        if (item != null)
        {
            accessControlList.Remove(item);
        }

        var response = fileClient.SetAccessControlList(accessControlList);

        await LoadSharedUsers(file);

        file.lastmodifiedts = DateTime.UtcNow;
        fileClient.SetMetadata(file.GenerateMetadata());
        //await _cognitiveSearchService.EditDocument(file);

        return response.GetRawResponse().Status == 200;
    }

    public async Task LoadSharedUsers(FileMetaData file)
    {
        var accessControlTuple = await GetAccessControlList(file);
        var accessControlList = accessControlTuple.Item1;

        file.sharedwith.Clear();

        foreach (var item in accessControlList.Where(i => i.AccessControlType == AccessControlType.User && !string.IsNullOrEmpty(i.EntityId) && i.EntityId != file.ownedby))
        {
            Sharedwith sharedwith = new Sharedwith
            {
                userid = item.EntityId,
                role = item.Permissions.HasFlag(RolePermissions.Write) ? "Editor" : "Viewer"
            };
            file.sharedwith.Add(sharedwith);
        }
    }

    private async Task<(List<PathAccessControlItem>, DataLakeFileClient)> GetAccessControlList(FileMetaData fileMetadata)
    {
        await CheckClients();

        DataLakeDirectoryClient directoryClient = dataLakeFileSystemClient.GetDirectoryClient(fileMetadata.folderpath);
        DataLakeFileClient fileClient = directoryClient.GetFileClient(fileMetadata.filename);
        PathAccessControl fileAccessControl = await fileClient.GetAccessControlAsync();

        return (fileAccessControl.AccessControlList.ToList(), fileClient);
    }

    private async Task CheckClients()
    {
        if (dataLakeFileSystemClient == null)
        {
            await SetDataLakeServiceClient();
        }

        if (_projectServiceClients == null)
        {
            _projectServiceClients = new Dictionary<string, DataLakeServiceClient>();
        }

    }
        
}