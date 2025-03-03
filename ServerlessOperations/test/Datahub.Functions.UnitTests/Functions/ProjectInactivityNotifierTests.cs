using Azure.Messaging.ServiceBus;
using Datahub.Application.Services;
using Datahub.Application.Services.Projects;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Functions.Providers;
using Datahub.Functions.Services;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Shared.Entities;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using System.Text.Json;

namespace Datahub.Functions.UnitTests.Functions;

public class ProjectInactivityNotifierTests
{
    private ProjectInactivityNotifier _sut;

    private readonly IDateProvider _dateProvider = Substitute.For<IDateProvider>();
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();

    private readonly IResourceMessagingService _resourceMessagingService = Substitute.For<IResourceMessagingService>();

    private readonly IProjectInactivityNotificationService _projectInactivityNotificationService =
        Substitute.For<IProjectInactivityNotificationService>();


    private readonly IConfiguration _config = Substitute.For<IConfiguration>();

    private AzureConfig _azConfig;
    private QueuePongService _pongService;
    private EmailValidator _emailValidator;
    private IEmailService _emailService;
    private ISendEndpointProvider _iSendEndpointProvider;
    private IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

    [SetUp]
    public async Task Setup()
    {
        _iSendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        _azConfig = new AzureConfig(_config);
        _pongService = new QueuePongService(_iSendEndpointProvider);
        _emailValidator = new EmailValidator();
        _emailService = new EmailService(_loggerFactory.CreateLogger<EmailService>());
        _dbContextFactory = TestHelper.CreateMockDbContextFactory();
        await TestHelper.SeedDatabase(_dbContextFactory);

        _sut = new ProjectInactivityNotifier(_loggerFactory, _dbContextFactory, _pongService, _iSendEndpointProvider,
            _projectInactivityNotificationService, _emailValidator, _dateProvider, _azConfig, _emailService);
    }

    [Test]
    [TestCase(10, new[] { 2, 1 })]
    [TestCase(10, new[] { 20, 5 })]
    [TestCase(10, new[] { 30, 20 })]
    public async Task CheckIfProjectToBeNotified_NotInNotificationDays(
        int daysUntilDeletion, int[] notificationDays)
    {
        // Arrange
        _dateProvider.Today.Returns(new DateTime(2000, 1, 1));
        _dateProvider.ProjectNotificationDays().Returns(notificationDays);

        // Act
        var result = await _sut.CheckIfProjectToBeNotified(10, daysUntilDeletion, null,
            false, "", new List<string>());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    [TestCase(10, new[] { 10, 1 })]
    [TestCase(10, new[] { 20, 10 })]
    [TestCase(10, new[] { 30, 20, 10 })]
    public async Task CheckIfProjectToBeNotified_InNotificationDays(
        int daysUntilDeletion, int[] notificationDays)
    {
        // Arrange
        _dateProvider.Today.Returns(new DateTime(2000, 1, 1));
        _dateProvider.ProjectNotificationDays().Returns(notificationDays);

        // Act
        var result = await _sut.CheckIfProjectToBeNotified(daysUntilDeletion, 10, null,
            false, "", new List<string>());

        // Assert
        result.Should().BeOfType<EmailRequestMessage>();
    }

    [Test]
    [TestCase("2025-01-01", "2020-01-01")]
    [TestCase("2020-01-01", "2020-01-01")]
    public async Task CheckIfProjectToBeNotified_InOperationalWindow(DateTime operationalWindow, DateTime today)
    {
        // Arrange
        _dateProvider.Today.Returns(today);
        _dateProvider.ProjectNotificationDays().Returns(new[] { 10 });

        // Act
        var result = await _sut.CheckIfProjectToBeNotified(10, 10, operationalWindow,
            false, "", new List<string>());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task CheckIfProjectToBeNotified_HasCostRecovery()
    {
        // Arrange
        _dateProvider.Today.Returns(DateTime.Today);
        _dateProvider.ProjectNotificationDays().Returns(new[] { 10 });

        // Act
        var result = await _sut.CheckIfProjectToBeNotified(10, 10, null,
            true, "", new List<string>());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    [TestCase(20, 30)]
    [TestCase(0, 1)]
    public void CheckIfProjectToBeDeleted_IsNotOrPastDeletionDay(int daysSinceLastLogin, int deletionDay)
    {
        // Arrange
        _dateProvider.ProjectSoftDeletionDay().Returns(deletionDay);
        _dateProvider.Today.Returns(new DateTime(2000, 1, 1));
        _resourceMessagingService.GetWorkspaceDefinition("").ReturnsForAnyArgs(new WorkspaceDefinition());

        // Act
        var result = _sut.CheckIfProjectToBeDeleted(daysSinceLastLogin, null, false);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    [TestCase(30, 30)]
    [TestCase(0, 0)]
    [TestCase(40, 30)]
    public void CheckIfProjectToBeDeleted_IsOrPastDeletionDay(int daysSinceLastLogin, int deletionDay)
    {
        // Arrange
        _dateProvider.ProjectSoftDeletionDay().Returns(deletionDay);
        _dateProvider.Today.Returns(new DateTime(2000, 1, 1));
        _resourceMessagingService.GetWorkspaceDefinition("").ReturnsForAnyArgs(new WorkspaceDefinition());

        // Act
        var result = _sut.CheckIfProjectToBeDeleted(daysSinceLastLogin, null, false);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    [TestCase("2025-01-01", "2020-01-01")]
    [TestCase("2020-01-01", "2020-01-01")]
    public void CheckIfProjectToBeDeleted_InOperationalWindow(DateTime operationalWindow, DateTime today)
    {
        // Arrange
        _dateProvider.Today.Returns(today);
        _dateProvider.ProjectSoftDeletionDay().Returns(10);
        _resourceMessagingService.GetWorkspaceDefinition("").ReturnsForAnyArgs(new WorkspaceDefinition());

        // Act
        var result = _sut.CheckIfProjectToBeDeleted(10, operationalWindow, false);

        result.Should().BeFalse();
    }

    [Test]
    public void CheckIfProjectToBeDeleted_HasCostRecovery()
    {
        // Arrange
        _dateProvider.Today.Returns(DateTime.Today);
        _dateProvider.ProjectSoftDeletionDay().Returns(10);
        _resourceMessagingService.GetWorkspaceDefinition("").ReturnsForAnyArgs(new WorkspaceDefinition());

        // Act
        var result = _sut.CheckIfProjectToBeDeleted(10, null, true);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GetEmailRequestMessage_ShouldHaveCorrectBody()
    {
        // Arrange
        var template = "project_inactive_alert.html";
        // Act
        var result = _sut.GetEmailRequestMessage(10, 20, "TEST", new List<string>(), template);

        // Assert
        result.Body.Should().Contain("Your workspace <a href=\"https://federal-science-datahub.canada.ca/w/TEST\">TEST</a> has been inactive for 20 days");
    }

    [Test]
    public async Task Run_ShouldReturnOkObjectResult_WhenRequestIsValid()
    {
        // Arrange
        var request = new ProjectInactivityNotificationMessage(4);
        
        var messageEnvelope = new
        {
            message = request
        };
        var messageBody = JsonSerializer.Serialize(messageEnvelope);
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData(messageBody));

        // Act
        Func<Task> act = async () => await _sut.Run(serviceBusReceivedMessage, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task Run_ShouldReturnBadRequestObjectResult_WhenRequestIsInvalid()
    {
        // Arrange
        var request = new ProjectInactivityNotificationMessage(100);

        var messageEnvelope = new
        {
            message = request
        };
        var messageBody = JsonSerializer.Serialize(messageEnvelope);
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: new BinaryData(messageBody));
      
        // Act
        Func<Task> act = async () => await _sut.Run(serviceBusReceivedMessage, CancellationToken.None);


        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _loggerFactory?.Dispose();
    }
}