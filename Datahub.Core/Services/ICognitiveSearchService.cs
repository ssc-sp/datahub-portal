using System.Threading.Tasks;
using Datahub.Core.Data;

namespace Datahub.Core.Services
{
    public interface ICognitiveSearchService
    {
        Task AddDocumentToIndex(FileMetaData fileMetaData);
        Task DeleteDocumentFromIndex(string documentId);
        Task EditDocument(FileMetaData fileMetaData);
        Task RunIndexerAsnyc();
    }
}