using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datahub.Infrastructure.Services.Azure;

public static class HttpClientExtensions
{
    public static async Task<ResponseType?> PostAsync<ResponseType>(this HttpClient httpClient, string url, string? accessToken, HttpContent? content, CancellationToken cancellationToken) where ResponseType : class
    {
        return await ExecuteQueryAsync<ResponseType>(httpClient, HttpMethod.Post, url, accessToken, content, cancellationToken);
    }

    public static async Task<ResponseType?> GetAsync<ResponseType>(this HttpClient httpClient, string url, string? accessToken, HttpContent? content, CancellationToken cancellationToken) where ResponseType : class
    {
        return await ExecuteQueryAsync<ResponseType>(httpClient, HttpMethod.Get, url, accessToken, content, cancellationToken);
    }

    static async Task<ResponseType?> ExecuteQueryAsync<ResponseType>(HttpClient httpClient, HttpMethod method, string url, string? accessToken, HttpContent? content, CancellationToken cancellationToken) where ResponseType : class
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

        var responseMessage = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<ResponseType>(responseContent, GetJsonSerializerOptions());
    }

    static JsonSerializerOptions GetJsonSerializerOptions() => new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
}
