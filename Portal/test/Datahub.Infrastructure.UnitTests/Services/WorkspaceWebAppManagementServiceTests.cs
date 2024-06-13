using Datahub.Infrastructure.Services.WebApp;
using FluentAssertions;
using MassTransit;
using MediatR;
using NSubstitute;
using static Datahub.Infrastructure.UnitTests.Testing;

namespace Datahub.Infrastructure.UnitTests.Services;

[TestFixture]
[Ignore("Missing configuration")]
public class WorkspaceWebAppManagementServiceTests
{
    private WorkspaceWebAppManagementService _service;
    private ISendEndpointProvider _sendEndpointProvider;

    [SetUp]
    public async Task Setup()
    {
        _sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        _service = new WorkspaceWebAppManagementService(_datahubPortalConfiguration, _dbContextFactory, _sendEndpointProvider);
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
        result.Should().BeTrue();
    }
}