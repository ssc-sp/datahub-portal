using System.Threading.Tasks;
using NRCan.Datahub.Shared.Data;

namespace NRCan.Datahub.Shared.Services
{
    public interface ICognitiveSearchService
    {
        Task AddDocumentToIndex(FileMetaData fileMetaData);
        Task DeleteDocumentFromIndex(string documentId);
        Task EditDocument(FileMetaData fileMetaData);
        Task RunIndexerAsnyc();
    }
}