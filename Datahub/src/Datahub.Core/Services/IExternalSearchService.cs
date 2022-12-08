using System.Threading.Tasks;
using Datahub.Core.Data.External.FGP;
using Datahub.Core.Data.External.OpenData;

namespace Datahub.Core.Services
{
    public interface IExternalSearchService
    {
        public Task<GeoCoreSearchResult> SearchFGPByKeyword(string keyword, int min = 1, int max = 10, string lang = "en");
        public Task<OpenDataResult> SearchOpenDataByKeyword(string keyword, int min = 0, int rows = 10);
    }
}