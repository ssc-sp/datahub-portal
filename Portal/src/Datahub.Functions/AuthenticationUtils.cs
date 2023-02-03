using System.Text.Json;

namespace Datahub.Functions;

internal class AuthenticationUtils
{
    private readonly AzureConfig _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthenticationUtils(AzureConfig config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string?> GetAccessTokenAsync(string audience)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, $"{_config.LoginUrl}/{_config.TenantId}/oauth2/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _config.ClientId },
                { "client_secret", _config.ClientSecret },
                { "resource", audience }
            })
        };
        
        var tokenResponse = await httpClient.SendAsync(tokenRequest);
        var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenResponseJson = JsonDocument.Parse(tokenResponseContent);

        return tokenResponseJson?.RootElement.GetProperty("access_token").GetString();
    }
}
