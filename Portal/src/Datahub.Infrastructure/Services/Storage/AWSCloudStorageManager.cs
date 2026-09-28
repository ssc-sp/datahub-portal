using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Datahub.Core.Data;
using Datahub.Core.Storage;
using Datahub.Infrastructure.Services.Security;
using Datahub.Portal.Pages.Workspace.Storage.ResourcePages;
using Datahub.Shared.Entities;

namespace Datahub.Infrastructure.Services.Storage;

public class AWSCloudStorageManager : ICloudStorageManager
{
	private readonly string _containerName;
	private readonly string _accessKeyId;
	private readonly string _secretAccessKey;
	private readonly string _region;
	private readonly string _bucketName;

	public AWSCloudStorageManager(string containerName, string accessKeyId, string secretAccessKey, string region, string bucketName)
	{
		_containerName = containerName;
		_accessKeyId = accessKeyId;
		_secretAccessKey = secretAccessKey;
		_region = region;
		_bucketName = bucketName;
	}

	private async Task TestConnection()
	{
		using var s3Client = GetClient();
		var request = new ListObjectsV2Request()
		{
			BucketName = _bucketName,
			MaxKeys = 1
		};
		await s3Client.ListObjectsV2Async(request);
	}

	public async Task<List<string>> GetContainersAsync()
	{
		await TestConnection();
		return await Task.FromResult(new List<string>() { _containerName });
	}

    public async Task<DfsPage> GetDfsPagesAsync(string container, string folderPath, string? continuationToken = null)
    {
        var folders = new List<string>();
        var files = new List<PortalFileMetadata>();

        // Correct folder path
        folderPath = ToAWSFolder(folderPath);

        using var s3Client = GetClient();

        var request = new ListObjectsV2Request()
        {
            BucketName = _bucketName
        };

        ListObjectsV2Response response;
        do
        {
            response = await s3Client.ListObjectsV2Async(request);
            foreach (S3Object entry in response.S3Objects)
            {
                var (belongsToFolder, isFolder, relativePath) = AnalyseFolderItem(folderPath, entry.Key);

                if (!belongsToFolder || string.IsNullOrEmpty(relativePath))
                {
                    continue;
                }

                if (isFolder)
                {
                    folders.Add(RemoveSlash(entry.Key));
                    continue;
                }

                // Validate and populate FileMetaData
                PortalFileMetadata fileMetaData = new()
                {
                    id = entry.ETag,
                    name = relativePath,
                    lastmodifiedts = entry.LastModified ?? DateTime.UtcNow, // Convert nullable DateTime? to DateTime
                    filesize = entry.Size > 0 ? entry.Size.ToString() : "Unknown" // Handle 0 size gracefully
                };

                files.Add(fileMetaData);
            }

            request.ContinuationToken = response.NextContinuationToken;

        } while (response.IsTruncated == true);

        return new DfsPage(folders, files, continuationToken!);
    }

    public async Task<bool> CreateFolderAsync(string container, string currentWorkingDirectory, string folderName)
	{
		using var s3Client = GetClient();
		try
		{
			if (string.IsNullOrEmpty(folderName))
				return false;

			var request = new PutObjectRequest()
			{
				BucketName = _bucketName,
				Key = ToAWSFolder(folderName),
				InputStream = new MemoryStream()
			};
			await s3Client.PutObjectAsync(request);

			return true;
		}
		catch
		{
			return false;
		}
	}

	public Task<bool> DeleteFileAsync(string container, string filePath)
	{
		return DeleteObjectAsync(filePath);
	}

	public Task<bool> DeleteFolderAsync(string container, string folderPath)
	{
		return DeleteObjectAsync(ToAWSFolder(folderPath));
	}

	public async Task<Uri> DownloadFileAsync(string container, string filePath, string userName, IFileTokenService? fileTokenService = null)
	{
		using var s3Client = GetClient();
        EnsureAvailable(await GetFileArchiveStatusAsync(s3Client, filePath));

		var urlRequest = new GetPreSignedUrlRequest
		{
			BucketName = _bucketName,
			Key = filePath,
			Expires = DateTime.UtcNow.AddDays(1),
			Verb = HttpVerb.GET
		};

		var url = s3Client.GetPreSignedURL(urlRequest);
		Uri uri = new Uri(url);

		return uri;
	}

	public async Task<bool> FileExistsAsync(string container, string filePath)
	{
		using var s3Client = GetClient();
		try
		{
			var response = await s3Client.GetObjectAsync(new GetObjectRequest()
			{
				BucketName = _bucketName,
				Key = filePath
			});
			return true;
		}
		catch (Amazon.S3.AmazonS3Exception ex)
		{
			if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
				return false;

			//status wasn't not found, so throw the exception
			throw;
		}
	}

	public Task<Uri> GenerateSasTokenAsync(string container, TimeSpan timeSpan)
	{
		throw new NotImplementedException();
	}

	public Task<StorageMetadata> GetStorageMetadataAsync(string container)
	{
		StorageMetadata metadata = new()
		{
			Container = container
		};
		return Task.FromResult(metadata);
	}

    public async Task<Dictionary<string, int>> ListFoldersAsync(string container, string prefix = "")
    {
        using var s3Client = GetClient();
        return await ListFoldersAsync(s3Client, prefix);
    }

    internal async Task<Dictionary<string, int>> ListFoldersAsync(IAmazonS3 s3Client, string prefix)
    {
        var folderPrefix = IsRoot(prefix) ? "" : ToAWSFolder(prefix);
        var folders = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [folderPrefix] = 0
        };
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = folderPrefix
        };

        ListObjectsV2Response response;
        do
        {
            response = await s3Client.ListObjectsV2Async(request);
            foreach (var entry in response.S3Objects ?? new List<S3Object>())
            {
                var key = entry.Key;
                // S3 folders may exist only as prefixes of objects, without a marker object.
                for (var index = key.IndexOf('/', folderPrefix.Length); index >= 0;
                     index = key.IndexOf('/', index + 1))
                {
                    folders.TryAdd(key[..(index + 1)], 0);
                }

                // Objects ending in a slash represent folders, not files.
                if (!key.EndsWith('/'))
                {
                    var parent = key[..(key.LastIndexOf('/') + 1)];
                    folders[parent]++;
                }
            }

            request.ContinuationToken = response.NextContinuationToken;
        } while (response.IsTruncated == true);

        return folders;
    }

	public Task<List<FileMetadata>> SearchFilesAsync(string container, string folderPath, string searchTerm, CancellationToken cancellationToken, bool searchInContent = false)
	{
		throw new NotImplementedException();
	}

    public async Task<bool> RenameFileAsync(string container, string oldFilePath, string newFilePath)
    {
        using var s3Client = GetClient();
        return await RenameFileAsync(s3Client, oldFilePath, newFilePath);
    }

    internal async Task<bool> RenameFileAsync(IAmazonS3 s3Client, string oldFilePath, string newFilePath)
    {
        if (string.IsNullOrWhiteSpace(oldFilePath) || string.IsNullOrWhiteSpace(newFilePath))
            return false;
        if (string.Equals(oldFilePath, newFilePath, StringComparison.Ordinal))
            return true;

        try
        {
            var metadata = await s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = oldFilePath
            });
            // Renaming uses the same single-copy limit and archive availability rules as class changes.
            if (metadata.ContentLength > 5L * 1024 * 1024 * 1024
                || !string.IsNullOrEmpty(metadata.ServerSideEncryptionCustomerMethod?.Value)
                || string.IsNullOrEmpty(metadata.ETag))
                return false;
            EnsureAvailable(GetArchiveStatus(metadata));

            await s3Client.CopyObjectAsync(new CopyObjectRequest
            {
                SourceBucket = _bucketName,
                SourceKey = oldFilePath,
                DestinationBucket = _bucketName,
                DestinationKey = newFilePath,
                SourceVersionId = metadata.VersionId,
                ETagToMatch = metadata.ETag,
                StorageClass = metadata.StorageClass ?? S3StorageClass.Standard,
                MetadataDirective = S3MetadataDirective.COPY,
                TaggingDirective = TaggingDirective.COPY,
                ServerSideEncryptionMethod = metadata.ServerSideEncryptionMethod,
                ServerSideEncryptionKeyManagementServiceKeyId = metadata.ServerSideEncryptionKeyManagementServiceKeyId,
                BucketKeyEnabled = metadata.BucketKeyEnabled
            });

            // Delete only after a successful copy, and only if the source content is unchanged.
            await s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = oldFilePath,
                IfMatch = metadata.ETag
            });
            return true;
        }
        catch
        {
            // Keep the source (and any completed copy) when either operation fails.
            return false;
        }
    }

	public bool AzCopyEnabled => false;
	public bool DatabrickEnabled => true;

	public CloudStorageProviderType ProviderType => CloudStorageProviderType.AWS;

	public string DisplayName => _containerName;

	private const long MaxFileSize = 10 * 1024 * 1024 * 1024L; // 10GB

	public async Task<bool> UploadFileAsync(string container, PortalFileMetadata file, Action<long> progess)
	{
		using var s3Client = GetClient();
        using var transferUtility = new TransferUtility(s3Client);
        return await UploadFileAsync(transferUtility, file);
    }

    internal async Task<bool> UploadFileAsync(ITransferUtility transferUtility, PortalFileMetadata file)
    {
		try
		{
            await using var stream = file.BrowserFile.OpenReadStream(MaxFileSize);
            var folder = file.folderpath.Replace('\\', '/').Trim('/');
            var fullPath = string.IsNullOrEmpty(folder) ? file.filename : $"{folder}/{file.filename}";
			await transferUtility.UploadAsync(stream, _bucketName, fullPath);

			return true;
		}
		catch
		{
			return false;
		}
	}

	private AmazonS3Client GetClient() => new(_accessKeyId, _secretAccessKey, RegionEndpoint.GetBySystemName(_region));

	private async Task<bool> DeleteObjectAsync(string filePath)
	{
		using var s3Client = GetClient();
		try
		{
			var request = new DeleteObjectRequest()
			{
				BucketName = _bucketName,
				Key = filePath
			};
			var response = await s3Client.DeleteObjectAsync(request);
			return true;
		}
		catch
		{
			return false;
		}
	}

	static (bool Belongs, bool IsFolder, string relativePath) AnalyseFolderItem(string folderPath, string path)
	{
		var isRoot = IsRoot(folderPath);
		if (!isRoot && !path.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase))
		{
			return (false, false, path);
		}

		var pathLength = path.Length;
		var relativeStart = isRoot ? 0 : folderPath.Length;

		for (var i = relativeStart; i < pathLength; i++)
		{
			if (path[i] == '/')
			{
				var isLastChar = i == pathLength - 1;
				return (isLastChar, isLastChar, path.Substring(relativeStart, pathLength - relativeStart - 1));
			}
		}

		return (true, false, path[relativeStart..]);
	}

	static string ToAWSFolder(string path)
	{
		if (string.IsNullOrEmpty(path))
			return "/";

		if (!path.EndsWith("/"))
			return $"{path}/";

		return path;
	}

	static string RemoveSlash(string path) => path.EndsWith('/') ? path[..^1] : path;

	static bool IsRoot(string path) => string.IsNullOrEmpty(path) || path == "/";

	public List<(string Placeholder, string Replacement)> GetSubstitutions(string projectAcronym, CloudStorageContainer container)
	{
		return new List<(string, string)>
		{
			(ResourceSubstitutions.ProjectAcronym, projectAcronym),
			(ResourceSubstitutions.AWSS3Bucket, KeyVaultUserService.GetSecretNameForStorage(container.Id.Value, CloudStorageHelpers.AWS_BucketName)),
			(ResourceSubstitutions.AWSAccessKey, KeyVaultUserService.GetSecretNameForStorage(container.Id.Value, CloudStorageHelpers.AWS_AccesKeyId)),
			(ResourceSubstitutions.AWSAccessKeySecret, KeyVaultUserService.GetSecretNameForStorage(container.Id.Value, CloudStorageHelpers.AWS_AccessKeySecret)),
			(ResourceSubstitutions.AWSRegion, KeyVaultUserService.GetSecretNameForStorage(container.Id.Value, CloudStorageHelpers.AWS_Region))
		};
    }

    public async Task<string> GetFileStorageTierAsync(string container, string file)
    {
        using var s3Client = GetClient();
        return await GetFileStorageTierAsync(s3Client, file);
    }

    internal async Task<string> GetFileStorageTierAsync(IAmazonS3 s3Client, string file)
    {
        try
        {
            var response = await s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest()
            {
                BucketName = _bucketName,
                Key = file
            });
            return response.StorageClass?.Value ?? "STANDARD";
        }
        catch (Amazon.S3.AmazonS3Exception ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                return "Not Found";
            //status wasn't not found, so throw the exception
            throw;
        }
    }

    public async Task<bool> SetFileStorageTierAsync(string container, string file, string newTier)
    {
        using var s3Client = GetClient();
        return await SetFileStorageTierAsync(s3Client, file, newTier);
    }

    internal async Task<bool> SetFileStorageTierAsync(IAmazonS3 s3Client, string file, string newTier)
    {
        if (!GetFileStorageTiersList().Contains(newTier, StringComparer.Ordinal))
            throw new StorageTierChangeException("Unsupported AWS storage class.");

        GetObjectMetadataResponse metadata;
        try
        {
            metadata = await s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = file
            });
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "InvalidRequest")
        {
            throw new StorageTierChangeException("Unable to read this object's encryption settings. Customer-provided encryption keys are not supported.");
        }

        if (!string.IsNullOrEmpty(metadata.ServerSideEncryptionCustomerMethod?.Value))
            throw new StorageTierChangeException("Customer-provided encryption keys are not supported for storage class changes.");
        
        if ((metadata.StorageClass?.Value ?? "STANDARD") == newTier)
            return true;
        
        if (metadata.ContentLength > 5L * 1024 * 1024 * 1024)
            throw new StorageTierChangeException("Files larger than 5 GB must have their storage class changed using AWS tooling.");
        
        EnsureAvailable(GetArchiveStatus(metadata));

        var hasVersion = !string.IsNullOrEmpty(metadata.VersionId) && metadata.VersionId != "null";
        if (!hasVersion && string.IsNullOrEmpty(metadata.ETag))
            throw new StorageTierChangeException("Unable to verify the source file. Refresh the file list and try again.");

        var request = new CopyObjectRequest
        {
            SourceBucket = _bucketName,
            SourceKey = file,
            DestinationBucket = _bucketName,
            DestinationKey = file,
            StorageClass = S3StorageClass.FindValue(newTier),
            MetadataDirective = S3MetadataDirective.COPY,
            TaggingDirective = TaggingDirective.COPY,
            SourceVersionId = metadata.VersionId,
            ETagToMatch = hasVersion ? null : metadata.ETag,
            ServerSideEncryptionMethod = metadata.ServerSideEncryptionMethod,
            ServerSideEncryptionKeyManagementServiceKeyId = metadata.ServerSideEncryptionKeyManagementServiceKeyId,
            BucketKeyEnabled = metadata.BucketKeyEnabled
        };
        await s3Client.CopyObjectAsync(request);
        return true;
    }

    public List<string> GetFileStorageTiersList()
    {
        return new List<string> { "STANDARD", "STANDARD_IA", "ONEZONE_IA", "INTELLIGENT_TIERING", "GLACIER_IR", "GLACIER", "DEEP_ARCHIVE" };
    }

    public static string GetStorageClassLabel(string tier) => CloudStorageHelpers.GetStorageClassLabel(tier);

    public async Task<CloudStorageArchiveStatus> GetFileArchiveStatusAsync(string container, string file)
    {
        using var s3Client = GetClient();
        return await GetFileArchiveStatusAsync(s3Client, file);
    }

    internal async Task<CloudStorageArchiveStatus> GetFileArchiveStatusAsync(IAmazonS3 s3Client, string file)
    {
        var metadata = await s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
        {
            BucketName = _bucketName,
            Key = file
        });
        return GetArchiveStatus(metadata);
    }

    internal static CloudStorageArchiveStatus GetArchiveStatus(GetObjectMetadataResponse metadata)
    {
        if (metadata.RestoreInProgress == true)
            return new(CloudStorageArchiveState.Restoring);
        var archived = metadata.StorageClass?.Value is "GLACIER" or "DEEP_ARCHIVE"
            || metadata.ArchiveStatus?.Value is "ARCHIVE_ACCESS" or "DEEP_ARCHIVE_ACCESS";
        if (!archived)
            return new(CloudStorageArchiveState.Available);
        if (metadata.RestoreExpiration > DateTime.UtcNow)
            return new(CloudStorageArchiveState.Available, metadata.RestoreExpiration);
        return new(CloudStorageArchiveState.RestoreRequired);
    }

    private static void EnsureAvailable(CloudStorageArchiveStatus status)
    {
        if (status.State != CloudStorageArchiveState.Available)
            throw new StorageTierChangeException("This file must be restored using AWS tooling before it can be downloaded or its storage class changed.");
    }

    public async Task<IDictionary<string, string>> GetFileMetadataAsync(string container, string file)
    {
        throw new NotImplementedException();
    }

    public async Task SetFileMetadataAsync(string container, string file, Dictionary<string, string> metadata)
    {
        throw new NotImplementedException();
    }
}
