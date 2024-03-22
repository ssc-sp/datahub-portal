using Polly;
using Polly.Contrib.WaitAndRetry;
using System.Net;
using System.Text.Json;

namespace Datahub.Infrastructure.Services.Azure;

public static class HttpClientExtensions
{
    static readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy =
            Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(x => x.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5));

    public static async Task<ResponseType?> PostAsync<ResponseType>(this HttpClient httpClient, string url, string? accessToken, HttpContent? content, CancellationToken cancellationToken) where ResponseType : class
    {
        return await ExecuteQueryAsync<ResponseType>(httpClient, HttpMethod.Post, url, accessToken, content, cancellationToken);
    }

    public static async Task<ResponseType?> GetAsync<ResponseType>(this HttpClient httpClient, string url, string? accessToken, HttpContent? content, CancellationToken cancellationToken) where ResponseType : class
    {
        return await ExecuteQueryAsync<ResponseType>(httpClient, HttpMethod.Get, url, accessToken, content, cancellationToken);
    }

    static async Task<ResponseType?> ExecuteQueryAsync<ResponseType>(HttpClient httpClient, HttpMethod method, 
        string url, string? accessToken, HttpContent? content, CancellationToken ct) where ResponseType : class
    {
        var httpRequest = new HttpRequestMessage(method, url);

        if (accessToken is not null)
        {
            httpRequest.Headers.Add("authorization", $"Bearer {accessToken}");
        }

        if (content is not null)
        {
            httpRequest.Content = content;
        }

        var responseMessage = await httpClient.SendAsync(httpRequest, ct);
        var responseContent = await responseMessage.Content.ReadAsStringAsync(ct);

        return JsonSerializer.Deserialize<ResponseType>(responseContent, GetJsonSerializerOptions());
    }

    static JsonSerializerOptions GetJsonSerializerOptions() => new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
}
