using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ResourceProvisioner.Infrastructure.Services;


namespace ResourceProvisioner.Application.IntegrationTests;

[SetUpFixture]
public partial class Testing
{
    private static WebApplicationFactory<Program> _factory = null!;
    private static IServiceScopeFactory _scopeFactory = null!;
    private static string? _currentUserId;

    internal static string GenerateRemoteTestName(string purpose)
    {
        return $"TEST-{purpose}-{Guid.NewGuid().ToString("N")[..8]}";
    }

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        _factory = new CustomWebApiFactory();
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        return await mediator.Send(request);
    }

    public static string? GetCurrentUserId()
    {
        return _currentUserId;
    }

    public static async Task<string> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync("test@local.com", "Testing1234!", Array.Empty<string>());
    }

    public static async Task<string> RunAsAdministratorAsync()
    {
        return await RunAsUserAsync("administrator@local.com", "Administrator1234!", new[] { "Administrator" });
    }

    public static async Task<string> RunAsUserAsync(string userName, string pat, string[] roles)
    {
        _currentUserId = userName;
        return _currentUserId;
    }

    internal static async Task AbandonPullRequest(int pullRequestId)
    {
        var configuration = _factory.Services
            .GetRequiredService<IConfiguration>();

        var httpClientFactory = _factory.Services
            .GetRequiredService<IHttpClientFactory>();

        var httpClient = httpClientFactory.CreateClient(
            RepositoryService.HttpClientName);

        var pullRequestUrl =
            configuration["InfrastructureRepository:PullRequestUrl"];

        var apiVersion =
            configuration["InfrastructureRepository:ApiVersion"];

        var patchUrl =
            $"{pullRequestUrl}/{pullRequestId}?api-version={apiVersion}";

        var patchData = new JsonObject
        {
            ["status"] = "abandoned"
        };

        using var patchContent = new StringContent(
            JsonSerializer.Serialize(patchData),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.PatchAsync(
            patchUrl,
            patchContent);

        response.EnsureSuccessStatusCode();
    }

    internal static async Task DeleteRemoteBranch(string branchName)
    {
        if (!branchName.StartsWith(
                "TEST-PR-",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Refusing to delete non-test branch {branchName}");
        }

        var configuration = _factory.Services
            .GetRequiredService<IConfiguration>();

        var httpClientFactory = _factory.Services
            .GetRequiredService<IHttpClientFactory>();

        var httpClient = httpClientFactory.CreateClient(
            RepositoryService.HttpClientName);

        var pullRequestUrl =
            configuration["InfrastructureRepository:PullRequestUrl"]
            ?? throw new InvalidOperationException(
                "InfrastructureRepository:PullRequestUrl is not configured");

        var apiVersion =
            configuration["InfrastructureRepository:ApiVersion"];

        var refsUrl = pullRequestUrl.Replace(
            "/pullrequests",
            "/refs",
            StringComparison.OrdinalIgnoreCase);

        var branchFilter = Uri.EscapeDataString(
            $"heads/{branchName}");

        var getResponse = await httpClient.GetAsync(
            $"{refsUrl}?filter={branchFilter}&api-version={apiVersion}");

        getResponse.EnsureSuccessStatusCode();

        var getContent =
            await getResponse.Content.ReadAsStringAsync();

        var getData =
            JsonSerializer.Deserialize<JsonNode>(getContent);

        var branch = getData?["value"]?
            .AsArray()
            .FirstOrDefault(node =>
                node?["name"]?.ToString() ==
                $"refs/heads/{branchName}");

        var oldObjectId =
            branch?["objectId"]?.ToString();

        if (string.IsNullOrWhiteSpace(oldObjectId))
        {
            return;
        }

        var deleteData = new JsonArray
        {
            new JsonObject
            {
                ["name"] = $"refs/heads/{branchName}",
                ["oldObjectId"] = oldObjectId,
                ["newObjectId"] =
                    "0000000000000000000000000000000000000000"
            }
        };

        using var deleteContent = new StringContent(
            JsonSerializer.Serialize(deleteData),
            Encoding.UTF8,
            "application/json");

        var deleteResponse = await httpClient.PostAsync(
            $"{refsUrl}?api-version={apiVersion}",
            deleteContent);

        deleteResponse.EnsureSuccessStatusCode();

        var deleteResponseContent =
            await deleteResponse.Content.ReadAsStringAsync();

        var deleteResponseData =
            JsonSerializer.Deserialize<JsonNode>(
                deleteResponseContent);

        var deleteResult = deleteResponseData?["value"]?
            .AsArray()
            .FirstOrDefault();

        if (deleteResult?["success"]?.GetValue<bool>() != true)
        {
            var updateStatus =
                deleteResult?["updateStatus"]?.ToString();

            throw new InvalidOperationException(
                $"Could not delete branch {branchName}. " +
                $"Azure DevOps status: {updateStatus}");
        }
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
        _factory.Dispose();
    }
}
