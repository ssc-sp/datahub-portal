using System.Web;
using Datahub.Core.Data.ExternalSearch.FGP;
using Datahub.Core.Data.ExternalSearch.OpenData;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Datahub.Core.Services.Search;

public class ExternalSearchService : IExternalSearchService
{
    private static readonly string FGP_SEARCH_API_URL = "https://hqdatl0f6d.execute-api.ca-central-1.amazonaws.com/dev/geo";
    private static readonly string OPEN_DATA_SEARCH_API_URL = "https://open.canada.ca/data/en/api/3/action/package_search";

    private readonly HttpClient _httpClient;

    private readonly ILogger<ExternalSearchService> _logger;

    public ExternalSearchService(ILogger<ExternalSearchService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<GeoCoreSearchResult> SearchFGPByKeyword(string keyword, int min = 1, int max = 10, string lang = "en")
    {
        _logger.LogDebug($"Searching FGP with keyword '{keyword}' (min: {min} , max: {max}, lang: {lang})");

        var encKeyword = HttpUtility.UrlEncode(keyword);

        try
        {
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri($"{FGP_SEARCH_API_URL}?keyword_only=true&lang={lang}&keyword={encKeyword}&min={min}&max={max}");

                _logger.LogTrace($"URI: {request.RequestUri}");

                using (var response = await _httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GeoCoreSearchResult>(content);

                    _logger.LogDebug($"Got {result.Count} FGP results for '{keyword}'");

                    return result;
                }
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, $"HTTP Error searching FGP for '{keyword}'");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error searching FGP for '{keyword}'");
            throw;
        }
    }

    public async Task<OpenDataResult> SearchOpenDataByKeyword(string keyword, int min = 0, int rows = 10)
    {
        _logger.LogDebug($"Searching Open Data with keyword '{keyword}' (min: {min} , rows: {rows})");
        var encKeyword = HttpUtility.UrlEncode(keyword);

        try
        {
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri($"{OPEN_DATA_SEARCH_API_URL}?q={keyword}&start={min}&rows={rows}");

                _logger.LogTrace($"Uri: {request.RequestUri}");

                using (var response = await _httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var wrapper = JsonConvert.DeserializeObject<OpenDataResultWrapper>(content);

                    //TODO check wrapper.success
                    return wrapper.Result;
                }
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, $"HTTP Error searching Open Data for '{keyword}'");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error searching Open Data for '{keyword}'");
            throw;
        }
    }
}