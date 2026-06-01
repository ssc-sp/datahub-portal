using Datahub.Application.Services.Security;
using Datahub.Infrastructure.Services.WebApp;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static Datahub.Infrastructure.UnitTests.Testing;

namespace Datahub.Infrastructure.UnitTests.Services;

[TestFixture]
[Ignore("Missing tests")]
public class WorkspaceWebAppManagementServiceTests
{
    private WorkspaceWebAppManagementService _service;
    private ISendEndpointProvider _sendEndpointProvider;
    private IKeyVaultUserService _keyVaultUserService;
    private ILogger<WorkspaceWebAppManagementService> _logger;

    [SetUp]
    public async Task Setup()
    {
        _sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        _keyVaultUserService = Substitute.For<IKeyVaultUserService>();
        var tokenCredentialService = Substitute.For<ISystemTokenCredentialService>();
        _logger = Substitute.For<ILogger<WorkspaceWebAppManagementService>>();
        _keyVaultUserService.GetVaultName(Arg.Any<string>(), Arg.Any<string>()).Returns("");
        _service = new WorkspaceWebAppManagementService(_datahubPortalConfiguration, tokenCredentialService, _dbContextFactory, _sendEndpointProvider, _keyVaultUserService, _logger);
        await _service.Stop(TestWebAppId);
        await Task.Delay(1000);
    }
    
    [TearDown]
    public async Task Teardown()
    {
        await _service.Stop(TestWebAppId);
    }
    

    [Test]
    public async Task Start_ShouldReturnTrue_WhenWebAppStartsSuccessfully()
    {
        // Act
        var result = await _service.Start(TestWebAppId);

        // Assert
        result.Should().BeTrue();
    }
    
    [Test]
    public async Task Stop_ShouldReturnTrue_WhenWebAppStopsSuccessfully()
    {
        // Arrange
        await _service.Start(TestWebAppId);
        await Task.Delay(1000);
        
        // Act
        var result = await _service.Stop(TestWebAppId);

        // Assert
        result.Should().BeTrue();
    }
    
    [Test]
    public async Task Restart_ShouldReturnTrue_WhenWebAppRestartsSuccessfully()
    {
        // Arrange
        await _service.Start(TestWebAppId);
        await Task.Delay(1000);
        
        // Act
        var result = await _service.Restart(TestWebAppId);

        // Assert
        result.Should().BeTrue();
    }
    
    [Test]
    public async Task GetState_ShouldReturnTrue_WhenWebAppIsRunning()
    {
        // Arrange
        await _service.Start(TestWebAppId);
        await Task.Delay(1000);
        
        // Act
        var result = await _service.GetState(TestWebAppId);

        // Assert
        result.Should().BeTrue();
    }
    
    [Test]
    public async Task GetState_ShouldReturnFalse_WhenWebAppIsStopped()
    {
        // Act
        var result = await _service.GetState(TestWebAppId);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task GetAppSettings_ShouldReturnCorrectAppSettings()
    {
        // Act
        var result = await _service.GetAppSettings(TestWebAppId);
        
        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(4);
        result.Should().ContainKey("DOCKER_REGISTRY_SERVER_USERNAME");
        result["DOCKER_REGISTRY_SERVER_USERNAME"].Should().Be("fsdhdev");
    }
    
    [Test]
    public async Task GetAppSettings_ShouldReturnEmptyDictionary_WhenWebAppDoesNotExist()
    {
        // Act
        var result = await _service.GetAppSettings("invalid-web-app-id");
        
        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(0);
    }
}
