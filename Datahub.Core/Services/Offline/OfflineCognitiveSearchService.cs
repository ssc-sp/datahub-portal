using System.Threading.Tasks;
using Datahub.Core.Data;

namespace Datahub.Core.Services
{
    public class OfflineCognitiveSearchService : ICognitiveSearchService
    {
        public OfflineCognitiveSearchService()
        {
        }

        public Task AddDocumentToIndex(FileMetaData fileMetaData)
        {
            return Task.FromResult(0);
        }

        public Task DeleteDocumentFromIndex(string documentId)
        {
            return Task.FromResult(0);
        }

        public Task EditDocument(FileMetaData fileMetaData)
        {
            return Task.FromResult(0);
        }

        public Task RunIndexerAsnyc()
        {
            return Task.FromResult(0);
        }
    }
}