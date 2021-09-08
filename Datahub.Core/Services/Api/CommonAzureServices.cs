using System;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NRCan.Datahub.Shared.Data;
using NRCan.Datahub.Shared.Services;

public class CommonAzureServices
{    
    private IOptions<APITarget> _targets;
    private readonly ILogger _logger;
    private readonly IKeyVaultService _keyVaultService;

    public CommonAzureServices(ILogger<CommonAzureServices> logger,
                               IKeyVaultService keyVaultService,
                               IOptions<APITarget> targets)
    {
        _logger = logger;
        _keyVaultService = keyVaultService;
        _targets = targets;
    }

    public async Task<AzureKeyCredential> GetSearchCredentials()
    {
        var creds = await _keyVaultService.GetSecret("Datahub-Search-Secret");

        return new AzureKeyCredential(creds);
    }
    public string LoginUrl
    {
        get
        {
            return _targets.Value.LoginURL;
        }
    }

    public string AzureSearchUri
    {
        get
        {
            return $"https://{_targets.Value.SearchServiceName}.search.windows.net";
        }
    }
    public string StorageAccountName
    {
        get
        {
            return _targets.Value.StorageAccountName;
        }
    }
    public string FileSystemName
    {
        get
        {
            return _targets.Value.FileSystemName;
        }
    }

    public async Task<SearchIndexClient> GetSearchIndexClient()
    {
        var azureKeyCreds = await GetSearchCredentials();
        return new SearchIndexClient(new Uri(AzureSearchUri), azureKeyCreds);
    }

    public async Task<SearchIndexerClient> GetCognitiveSearchIndexerClient()
    {
        var azureKeyCreds = await GetSearchCredentials();
        return new SearchIndexerClient(new Uri(_targets.Value.CognitiveSearchURL), azureKeyCreds);
    }

    public async Task<SearchClient> GetSearchClient()
    {
        var searchIndexClient = await GetSearchIndexClient();
        var searchClient = searchIndexClient.GetSearchClient(_targets.Value.FileIndexName);

        return searchClient;
    }
    public async Task<SearchClient> GetSearchClientForIndexing()
    {
        var azureKeyCreds = await GetSearchCredentials();

        return new SearchClient(new Uri(AzureSearchUri), _targets.Value.FileIndexName, azureKeyCreds);
    }
}