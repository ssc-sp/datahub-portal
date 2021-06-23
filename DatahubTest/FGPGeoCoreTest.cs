using System;
using System.Net.Http;
using Newtonsoft.Json;
using Xunit;

namespace DatahubTest
{
    public class FGPGeoCoreTest
    {
        private static readonly string SEARCH_API_URL = "https://hqdatl0f6d.execute-api.ca-central-1.amazonaws.com/dev/geo";

        [Fact]
        public async void TestKeywordQuery() 
        {
            var keyword = "ferroalloy";
            var httpClient = new HttpClient();
            
            using (var request = new HttpRequestMessage())
            {
                request.Method=HttpMethod.Get;
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
    }
}