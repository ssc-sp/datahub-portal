using System.Threading.Tasks;
using NRCan.Datahub.Shared.Data.External.FGP;
using NRCan.Datahub.Shared.Data.External.OpenData;

namespace NRCan.Datahub.Shared.Services
{
    public interface IExternalSearchService
    {
        public Task<GeoCoreSearchResult> SearchFGPByKeyword(string keyword, int min = 1, int max = 10, string lang = "en");
        public Task<OpenDataResult> SearchOpenDataByKeyword(string keyword, int min = 0, int rows = 10);
    }
}