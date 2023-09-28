using Datahub.Application.Services;
using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services;
using Datahub.Core.Services.Notification;
using Datahub.ProjectTools.Services;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Moq;

namespace Datahub.ProjectTools.Tests;

public class RequestManagementServiceTest
{


    [Test]
    public async Task ShouldGenerateCorrectJsonMessageTest()
    {
        var loggerMock = new Mock<ILogger<RequestManagementService>>();
        var dbContextMock = new Mock<DbContextFactory<DatahubProjectDBContext>>();
        var toolService = new Mock<ProjectToolsEmailService>();
        var emailMock = new Mock<IEmailNotificationService>();
        var systemNotificationMock = new Mock<ISystemNotificationService>();
        var userInformationMock = new Mock<IUserInformationService>();
        var auditingMock = new Mock<IDatahubAuditingService>();
        var resourceRequestServiceMock = new Mock<IResourceMessagingService>();
        var miscStorageMock = new Mock<IMiscStorageService>();
        
        
        var requestManagementService = new RequestManagementService(
            loggerMock.Object, dbContextMock.Object, toolService.Object, systemNotificationMock.Object, userInformationMock.Object, auditingMock.Object, resourceRequestServiceMock.Object, miscStorageMock.Object);

        var project = new Datahub_Project
        {

        };
        var users = Enumerable.Range(0, 15)
            .Select(u => new TerraformUser() { ObjectId = Guid.NewGuid().ToString(), Email = $"{u}@email.com" })
            .ToList();

        var workspace = project.ToResourceWorkspace(users);
        var templates = new List<TerraformTemplate> { TerraformTemplate.Default };

        var request = CreateResourceData.ResourceRunTemplate(workspace, templates, "test@email.com");
        
        
    }
}