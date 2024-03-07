using Datahub.Infrastructure.Services.WebApp;
using MediatR;
using NSubstitute;
using static Datahub.Infrastructure.UnitTests.Testing;

namespace Datahub.Infrastructure.UnitTests.Services;

[TestFixture]
public class WorkspaceWebAppManagementServiceTests
{
    private WorkspaceWebAppManagementService _service;
    private IMediator _mediatr;

    [SetUp]
    public async Task Setup()
    {
        _mediatr = Substitute.For<IMediator>();
        _service = new WorkspaceWebAppManagementService(_datahubPortalConfiguration, _dbContextFactory, _mediatr);
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
        Assert.IsTrue(result);
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
        Assert.IsTrue(result);
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
        Assert.IsTrue(result);
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
        Assert.IsTrue(result);
    }
    
    [Test]
    public async Task GetState_ShouldReturnFalse_WhenWebAppIsStopped()
    {
        // Act
        var result = await _service.GetState(TestWebAppId);

        // Assert
        Assert.IsFalse(result);
    }
}