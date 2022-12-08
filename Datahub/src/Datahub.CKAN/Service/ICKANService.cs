using Datahub.Metadata.DTO;
using System.IO;
using System.Threading.Tasks;

namespace Datahub.CKAN.Service
{
    public interface ICKANService
    {
        Task<CKANApiResult> CreatePackage(FieldValueContainer fieldValues, bool allFields, string url = null);
        Task<CKANApiResult> AddResourcePackage(string packageId, string fileName, Stream fileContent);
        Task<CKANApiResult> DeletePackage(string packageId);
    }

    public record CKANApiResult(bool Succeeded, string ErrorMessage);
}
