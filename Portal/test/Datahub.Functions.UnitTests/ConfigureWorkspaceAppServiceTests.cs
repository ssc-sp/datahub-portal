using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Datahub.Core.Data;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services.Security;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Datahub.Functions.UnitTests;

[TestFixture]
public class ConfigureWorkspaceAppServiceTests
{
    private readonly ILogger<ConfigureWorkspaceAppService> _logger;
    private readonly IKeyVaultService _keyVaultService;
    private readonly AzureConfig _azureConfig;
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContext;
    private readonly ConfigureWorkspaceAppService _configureWorkspaceAppService;

    private const string ListPipelineUrlTemplate =
        "https://dev.azure.com/{organization}/{project}/_apis/pipelines?api-version=7.1-preview.1";

    public ConfigureWorkspaceAppServiceTests()
    {
        IOptions<APITarget> options = Substitute.For<IOptions<APITarget>>();
        DatahubPortalConfiguration portalConfiguration = new DatahubPortalConfiguration();
        _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.Test.json").Build();
        options.Value.Returns(new APITarget());
        options.Value.KeyVaultName.Returns(_configuration["KeyVaultName"]);
        portalConfiguration.AzureAd = _configuration.GetSection("AzureAd").Get<AzureAd>();
        portalConfiguration.PortalRunAsManagedIdentity = "false";

        _logger = Substitute.For<ILogger<ConfigureWorkspaceAppService>>();
        _keyVaultService = new KeyVaultCoreService(options, Substitute.For<ILogger<KeyVaultCoreService>>(),
            portalConfiguration);
        _azureConfig = Substitute.For<AzureConfig>();
        _dbContext = Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();
        _configureWorkspaceAppService = new ConfigureWorkspaceAppService(_logger, _keyVaultService, _azureConfig,
            _dbContext);
        _configureWorkspaceAppService._httpClient = Substitute.For<HttpClient>();
    }

    [Test]
    public async Task GetPipelineUrlByName_ShouldReturnCorrectUrl_GivenCorrectName()
    {
        // Arrange
        _azureConfig.AdoConfig.ListPipelineUrlTemplate.Returns(ListPipelineUrlTemplate);

        // Act
        var url = await _configureWorkspaceAppService.GetPipelineUrlByName("fsdh.wiki");
        var correct_url =
            "https://dev.azure.com/DataSolutionsDonnees/FSDH%20SSC/_apis/pipelines/10/runs?api-version=7.1-preview.1";

        // Assert
        Assert.That(url, Is.EqualTo(correct_url));
    }

    [Test]
    public async Task GetPipelineUrlByName_ShouldThrowError_GivenIncorrectUrl()
    {
        // Arrange
        _azureConfig.AdoConfig.ListPipelineUrlTemplate.Returns(
            ListPipelineUrlTemplate);

        // Act
        try
        {
            await _configureWorkspaceAppService.GetPipelineUrlByName("");
            Assert.Fail();
        }
        catch (ArgumentException e)
        {
            Assert.Pass();
        }
    }

    [Test]
    public async Task GetPipelineUrlByName_ShouldThrowError_WhenIncorrectPipelineUrl()
    {
        // Arrange
        _azureConfig.AdoConfig.ListPipelineUrlTemplate.Returns(
            "wrong url!");

        // Act
        try
        {
            await _configureWorkspaceAppService.GetPipelineUrlByName("");
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
        var appServiceConfiguration = new AppServiceConfiguration("test", "test", "test");
        var pipelineUrl = await _configureWorkspaceAppService.GetPipelineUrlByName("fsdh.wiki");
        var projectAcronym = "TEST";

        // Act
        var response =
            await _configureWorkspaceAppService.PostPipelineRun(pipelineUrl, appServiceConfiguration, projectAcronym);

        // Assert
        Assert.Equals(response.IsSuccessStatusCode, true);
    }

    [Test]
    public async Task PostPipelineRun_ShouldThrowError_GivenBadUrlOrConfiguration()
    {
        // Arrange
        var appServiceConfiguration = new AppServiceConfiguration("test", "test", "test");
        var pipelineUrl = "wrong url!";
        var projectAcronym = "TEST";

        // Act
        try
        {
            await _configureWorkspaceAppService.PostPipelineRun(pipelineUrl, appServiceConfiguration, projectAcronym);
            Assert.Fail();
        }
        catch (Exception e)
        {
            Assert.Pass();
        }
    }
}