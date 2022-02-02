using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Datahub.Core.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Datahub.Core.Services
{
    public class CognitiveSearchService : ICognitiveSearchService
    {
        private ILogger<CognitiveSearchService> _logger;
        private IKeyVaultService _keyVaultService;
        private IOptions<APITarget> _targets;
        private CommonAzureServices _commonAzureServices;

        public CognitiveSearchService(ILogger<CognitiveSearchService> logger,
                    IKeyVaultService keyVaultService,
                    CommonAzureServices commonAzureServices,
                    IOptions<APITarget> targets)
        {
            _logger = logger;
            _keyVaultService = keyVaultService;
            _targets = targets;
            _commonAzureServices = commonAzureServices;
        }

        public async Task RunIndexerAsnyc()
        {
            var indexerClient = await _commonAzureServices.GetCognitiveSearchIndexerClient();
            var response = await indexerClient.RunIndexerAsync(_targets.Value.FileIndexerName);

            _logger.LogInformation($"indexer response: {response.Status}");
        }

        public async Task DeleteDocumentFromIndex(string documentId)
        {
            var searchClient = await _commonAzureServices.GetSearchClient();

            List<string> idsToDelete = new List<string>() { documentId };
            var response = searchClient.DeleteDocuments("fileid", idsToDelete);
        }

        public async Task<bool> AddDocumentToIndex(FileMetaData fileMetaData)
        {
            try
            {
                var searchClient = await _commonAzureServices.GetSearchClient();

                IndexDocumentsBatch<FileMetaData> batch = IndexDocumentsBatch.Create(IndexDocumentsAction.Upload(fileMetaData));
                IndexDocumentsResult result = searchClient.IndexDocuments(batch);
                return true;
            } catch (Exception ex)
            {
                _logger.LogError(ex, $"Error indexing {fileMetaData}");
                return false;
            }
        }

        public async Task EditDocument(FileMetaData fileMetaData)
        {
            var searchClient = await _commonAzureServices.GetSearchClient();

            IndexDocumentsBatch<FileMetaData> batch = IndexDocumentsBatch.Create(IndexDocumentsAction.Merge(fileMetaData));
            IndexDocumentsResult result = searchClient.IndexDocuments(batch);
        }
    }
}