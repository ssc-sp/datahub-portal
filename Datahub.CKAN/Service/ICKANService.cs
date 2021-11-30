using Datahub.Metadata.DTO;
using System.Threading.Tasks;

namespace Datahub.CKAN.Service
{
    public interface ICKANService
    {
        Task<CKANApiResult> CreatePackage(FieldValueContainer fieldValues, string url);
        bool IsStaging();
    }

    public record CKANApiResult(bool Succeeded, string ErrorMessage);
}
