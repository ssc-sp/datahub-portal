using Datahub.Application.Services.Security;
using Datahub.Shared.Clients;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static Datahub.Functions.UnitTests.Testing;

namespace Datahub.Functions.UnitTests.Functions;

[TestFixture]
public class ConfigureWorkspaceAppServiceTests
{
    private readonly ILogger<ConfigureWorkspaceAppService> _logger;
    private readonly ConfigureWorkspaceAppService _configureWorkspaceAppService;

    private const string ListPipelineUrlTemplate =
        "https://dev.azure.com/{organization}/{project}/_apis/pipelines?api-version=7.1-preview.1";

    public ConfigureWorkspaceAppServiceTests()
    {
        _logger = Substitute.For<ILogger<ConfigureWorkspaceAppService>>();

        var _config = Substitute.For<IConfiguration>();
        _azureConfig = new AzureConfig(_config);

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var httpClient = new HttpClient();


        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        //_configureWorkspaceAppService = new ConfigureWorkspaceAppService(_logger, _azureConfig, _dbContext);
        var tokenCredentialService = Substitute.For<ISystemTokenCredentialService>();
        var tokenManager = Substitute.For<AzAccessTokenManager>(tokenCredentialService, tokenCredentialService);
        var config = Substitute.For<IAzureDevopsConfiguration>();
        var azClient = Substitute.For<AzureDevOpsClient>(config, tokenManager);

        _configureWorkspaceAppService = Substitute.For<ConfigureWorkspaceAppService>(_logger, _azureConfig, azClient, _dbContext);

        // Stub ConfigureHttpClient method
        _configureWorkspaceAppService.ConfigureHttpClient().Returns(Task.FromResult(httpClient));

    }

    [SetUp]
    public void Setup()
    {
        _azureConfig.AzureDevOpsConfiguration.ListPipelineUrlTemplate = ListPipelineUrlTemplate;
    }

    [Test]
    [Ignore("Need Azure auth to access URL")]
    public async Task GetPipelineIdByName_ShouldReturnCorrectId_GivenCorrectName()
    {
        // Act
        var id = await _configureWorkspaceAppService.GetPipelineIdByName("fsdh.wiki");
        var correct_id = 10;

        // Assert
        Assert.That(id, Is.EqualTo(correct_id));
    }

    [Test]
    public async Task GetPipelineIdByName_ShouldTrowError_GivenNonAuthorizedUser()
    {
        // Act & Assert
        try
        {
            await _configureWorkspaceAppService.GetPipelineIdByName("fsdh.wiki");
            Assert.Fail("Expected an exception due to unauthorized access.");
        }
        catch (Exception)
        {
            Assert.Pass("Caught expected unauthorized access exception.");
        }
    }

    [Test]
    public async Task GetPipelineIdByName_ShouldThrowError_GivenIncorrectUrl()
    {
        // Arrange
        _azureConfig.AzureDevOpsConfiguration.ListPipelineUrlTemplate = "https://INVALID_URL.com";

        // Act
        try
        {
            await _configureWorkspaceAppService.GetPipelineIdByName("");
            Assert.Fail();
        }
        catch (ArgumentException e)
        {
            Assert.Pass();
        }
    }

    [Test]
    public async Task GetPipelineIdByName_ShouldThrowError_WhenIncorrectPipelineUrl()
    {
        // Arrange
        _azureConfig.AzureDevOpsConfiguration.ListPipelineUrlTemplate = "https://INVALID_URL.com";

        // Act
        try
        {
            await _configureWorkspaceAppService.GetPipelineIdByName("");
            Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.Pass();
        }
    }

    [Test]
    public async Task PostPipelineRun_ShouldReturnGoodResponse_GivenGoodUrlAndConfiguration()
    {
        // Arrange
        var appServiceConfiguration = new AppServiceConfiguration
        {
            Framework = "test",
            GitRepo = "test",
            ComposePath = "test",
            Id = "/test"
        };
        var pipelineId = 101; 
        var projectAcronym = "TEST";

        // Act
        var response =
            await _configureWorkspaceAppService.PostPipelineRun(pipelineId, appServiceConfiguration, projectAcronym);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(true));
    }

    [Test]
    public async Task PostPipelineRun_ShouldThrowError_GivenBadUrlOrConfiguration()
    {
        // Arrange
        var appServiceConfiguration = new AppServiceConfiguration
        {
            Framework = "test",
            GitRepo = "test",
            ComposePath = "test"
        };
        var pipelineId = int.MaxValue;
        var projectAcronym = "TEST";

        // Act
        try
        {
            await _configureWorkspaceAppService.PostPipelineRun(pipelineId, appServiceConfiguration, projectAcronym);
            Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.Pass();
        }
    }
}
