using System.Threading.Tasks;
using Datahub.Shared.Data;

namespace Datahub.Shared.Services
{
    public interface ICognitiveSearchService
    {
        Task AddDocumentToIndex(FileMetaData fileMetaData);
        Task DeleteDocumentFromIndex(string documentId);
        Task EditDocument(FileMetaData fileMetaData);
        Task RunIndexerAsnyc();
    }
}