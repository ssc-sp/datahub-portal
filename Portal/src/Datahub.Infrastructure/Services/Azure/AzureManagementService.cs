namespace Datahub.Infrastructure.Services.Azure;

public class AzureManagementService
{
    public const string ClientName = "Azure Management";
    private readonly IAzureServicePrincipalConfig _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AzureManagementService(IAzureServicePrincipalConfig configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AzureManagementSession> GetSession(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(ClientName);
        var accessToken = await GetAccessTokenAsync(httpClient, AzureManagementUrls.ManagementUrl, cancellationToken);

        // validate access token
        if (accessToken is null)
        {
            throw new UnauthorizedAccessException("Cannot retrieve an access token with the configured service principal.");
        }

        return new AzureManagementSession(_configuration, httpClient, accessToken, cancellationToken);
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        return await GetAccessTokenAsync(default, AzureManagementUrls.ManagementUrl, cancellationToken);
    }

    private async Task<string?> GetAccessTokenAsync(HttpClient? httpClient, string audience, CancellationToken cancellationToken)
    {
        httpClient ??= _httpClientFactory.CreateClient();

        var url = $"{AzureManagementUrls.LoginUrl}/{_configuration.TenantId}/oauth2/token";
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _configuration.ClientId },
            { "client_secret", _configuration.ClientSecret },
            { "resource", audience }
        });
        var tokenResponse = await httpClient.PostAsync<AccessTokenResponse>(url, default, content, cancellationToken);
        return tokenResponse?.access_token;
    }
}

class AccessTokenResponse
{
    public string? token_type { get; set; }
    public string? expires_in { get; set; }
    public string? ext_expires_in { get; set; }
    public string? expires_on { get; set; }
    public string? not_before { get; set; }
    public string? resource { get; set; }
    public string? access_token { get; set; }
}