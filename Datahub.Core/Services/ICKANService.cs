using System.Collections.Generic;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Services
{
    public interface ICKANService
    {
        Task<CKANApiResult> CreatePackage(IDictionary<string, object> packageData);
    }

    public record CKANApiResult(bool Succeeded, string ErrorMessage);
}
