using Datahub.Shared.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;

using static Datahub.Functions.UnitTests.Testing;

namespace Datahub.Functions.UnitTests;

[TestFixture]
[Ignore("Missing configuration")]
public class ConfigureWorkspaceAppServiceTests
{
    private readonly ILogger<ConfigureWorkspaceAppService> _logger;
    private readonly ConfigureWorkspaceAppService _configureWorkspaceAppService;

    private const string ListPipelineUrlTemplate =
        "https://dev.azure.com/{organization}/{project}/_apis/pipelines?api-version=7.1-preview.1";

    public ConfigureWorkspaceAppServiceTests()
    {
        _logger = Substitute.For<ILogger<ConfigureWorkspaceAppService>>();
        _configureWorkspaceAppService = new ConfigureWorkspaceAppService(_logger, _azureConfig,
            _dbContext);
    }
    
    [SetUp]
    public void Setup()
    {
        _azureConfig.AzureDevOpsConfiguration.ListPipelineUrlTemplate = ListPipelineUrlTemplate;
    }

    [Test]
    public async Task GetPipelineIdByName_ShouldReturnCorrectId_GivenCorrectName()
    {
        // Act
        var id = await _configureWorkspaceAppService.GetPipelineIdByName("fsdh.wiki");
        var correct_id = 10;

        // Assert
        Assert.That(id, Is.EqualTo(correct_id));
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
        var appServiceConfiguration = new AppServiceConfiguration("test", "test", "test", id: "/test");
        var pipelineId = await _configureWorkspaceAppService.GetPipelineIdByName("fsdh.wiki");
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
        var appServiceConfiguration = new AppServiceConfiguration("test", "test", "test");
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