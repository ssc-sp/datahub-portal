using System;
using System.Net.Http;
using Datahub.Core.Data.ExternalSearch.FGP;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Datahub.Core.Services.Search;

namespace Datahub.Tests;

public class ExternalSearchFixture : IDisposable
{
    public IExternalSearchService ExternalSearchService { get; private set; }
    public ILogger Logger { get; private set; }

    private readonly HttpClient _httpClient;

    public ExternalSearchFixture()
    {
        _httpClient = new HttpClient();
    }

    // ITestOutputHelper can only be injected in a unit test class, not in a fixture
    // so this method is a hack to get around that
    // otherwise this initialization would be done in the constructor
    public void InitOutput<T>(ITestOutputHelper output) where T : class
    {
        Logger = output.BuildLoggerFor<T>();
        ExternalSearchService = new ExternalSearchService(output.BuildLoggerFor<ExternalSearchService>(), _httpClient);

    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

public class ExternalSearchTest : IClassFixture<ExternalSearchFixture>
{
    private static readonly string SEARCH_API_URL = "https://hqdatl0f6d.execute-api.ca-central-1.amazonaws.com/dev/geo";

    private ExternalSearchFixture _fixture;

    public ExternalSearchTest(ExternalSearchFixture fixture, ITestOutputHelper output)
    {
        fixture.InitOutput<ExternalSearchTest>(output);
        _fixture = fixture;
    }

    [Fact(Skip = "Not working")]
    public async void TestFGPKeywordQuery()
    {
        var keyword = "ferroalloy";
        var httpClient = new HttpClient();

        using (var request = new HttpRequestMessage())
        {
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{SEARCH_API_URL}?keyword_only=true&lang=en&keyword={keyword}");

            using (var response = await httpClient.SendAsync(request))
            {
                var content = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<dynamic>(content);

                Assert.Equal(1, (int)result.Count);

                var item = result.Items[0];

                Assert.Equal("Principal Mineral Areas, Producing Mines, and Oil and Gas Fields (900A)", (string)item.title);
            }
        }
    }

    [Fact(Skip = "Not working")]
    public async void TestFGPKeywordQueryToObject()
    {
        var keyword = "ferroalloy";
        var httpClient = new HttpClient();

        using (var request = new HttpRequestMessage())
        {
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{SEARCH_API_URL}?keyword_only=true&lang=en&keyword={keyword}");

            using (var response = await httpClient.SendAsync(request))
            {
                var content = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<GeoCoreSearchResult>(content);

                Assert.Equal(1, result.Count);

                var item = result.Items[0];

                Assert.Equal("Principal Mineral Areas, Producing Mines, and Oil and Gas Fields (900A)", item.Title);
            }
        }
    }

    [Fact(Skip = "Not working")]
    public async void TestSingleFGPResultFromService()
    {
        var keyword = "ferroalloy";
        _fixture.Logger.LogInformation($"Testing keyword '{keyword}' - should be a single result");

        var result = await _fixture.ExternalSearchService.SearchFGPByKeyword(keyword);
        Assert.Equal(1, result.Count);
        var item = result.Items[0];
        Assert.Equal("Principal Mineral Areas, Producing Mines, and Oil and Gas Fields (900A)", item.Title);
    }

    [Fact]
    public async void TestMultiFGPResultsFromService()
    {
        var keyword = "mining";
        _fixture.Logger.LogInformation($"Testing keyword '{keyword}' - should have multiple results, limited to 10 in query");

        var result = await _fixture.ExternalSearchService.SearchFGPByKeyword(keyword, min: 1, max: 10);
        Assert.Equal(10, result.Count);
    }

    [Fact(Skip = "Needs to be validated")]
    public async void TestSingleFGPResultFrench()
    {
        var keyword = "Pompage turbinage";
        _fixture.Logger.LogInformation($"Testing keyword '{keyword}' in French - should be a single result");

        var result = await _fixture.ExternalSearchService.SearchFGPByKeyword(keyword, lang: "fr");
        Assert.Equal(1, result.Count);
        var item = result.Items[0];
        Assert.Equal("Coopération nord-américaine en matière d’information sur l’énergie, données cartographiques", item.Title);

    }

    [Fact]
    public async void TestZeroFGPResultsFromService()
    {
        var keyword = "somethingthatdoesntexist";
        _fixture.Logger.LogInformation($"Testing keyword '{keyword}' - should get no results");

        var result = await _fixture.ExternalSearchService.SearchFGPByKeyword(keyword);
        Assert.Equal(0, result.Count);
    }

    [Fact(Skip = "Not working")]
    public async void TestFGPResultOutOfRange()
    {
        var keyword = "ferroalloy";
        _fixture.Logger.LogInformation($"Testing out of range search result with keyword '{keyword}' - should have one result, but querying with min > 1 returns none.");

        var result = await _fixture.ExternalSearchService.SearchFGPByKeyword(keyword, min: 2);
        Assert.Equal(0, result.Count);
    }

    [Fact(Skip = "Needs to be validated")]
    public async void TestFGPEmbeddedObjects()
    {
        var keyword = "ferroalloy";
        _fixture.Logger.LogInformation($"Testing keyword '{keyword}' embedded objects - contact, options and graphicOverview should match expected");

        var result = await _fixture.ExternalSearchService.SearchFGPByKeyword(keyword);
        Assert.Equal(1, result.Count);

        var item = result.Items[0];
        var contactList = item.ContactList;
        var optionsList = item.OptionsList;
        var graphicsList = item.GraphicOverviewList;

        Assert.Single(contactList);
        var contact = item.FirstContact;
        Assert.Equal("580 Booth Street", contact.Address.En);
        _fixture.Logger.LogInformation("Contact address matches");

        Assert.Equal(10, optionsList.Count);
        // options are populated in the same order each time, thankfully
        // first two have a "null" string in all the fields
        // the third one has something
        // if ever we update the class To ignore/remove the null ones, we may need To update this
        var option = optionsList[2];
        Assert.Equal("Vector dataset of the 900A map", option.Name.En);
        _fixture.Logger.LogInformation("Expected option matches");

        Assert.Single(graphicsList);
        var graphicOverview = item.FirstGraphicOverview;
        Assert.Equal(
            "http://ftp.maps.canada.ca/pub/nrcan_rncan/Mining-industry_Industrie-miniere/900A_and_top_100/Thumbnail/900A.png",
            graphicOverview.OverviewFilename);
        _fixture.Logger.LogInformation("GraphicOverview filename matches");
    }
}