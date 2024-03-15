using Datahub.Core.Data.ExternalSearch.FGP;
using Datahub.Core.Data.ExternalSearch.OpenData;

namespace Datahub.Core.Services.Search;

public interface IExternalSearchService
{
    public Task<GeoCoreSearchResult> SearchFGPByKeyword(string keyword, int min = 1, int max = 10, string lang = "en");
    public Task<OpenDataResult> SearchOpenDataByKeyword(string keyword, int min = 0, int rows = 10);
}