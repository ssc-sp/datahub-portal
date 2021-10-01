using System.Collections.Generic;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Services
{
    public interface ICKANService
    {
        Task<CKANApiResult> CreatePackage(IDictionary<string, object> packageData);
    }

    public struct CKANApiResult
    {
        public bool Succeeded { get; set; }
        public string Error {  get; set; }
    }
}
