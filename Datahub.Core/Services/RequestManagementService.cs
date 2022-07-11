using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Core.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Core.Services
{
    public class RequestManagementService
    {
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly ISystemNotificationService _systemNotificationService;
        private readonly IUserInformationService _userInformationService;
        private readonly IDatahubAuditingService _datahubAuditingService;
        private readonly IMiscStorageService _miscStorageService;

        public const string DATABRICKS = ProjectResourceConstants.SERVICE_TYPE_DATABRICKS;
        public const string POWERBI = ProjectResourceConstants.SERVICE_TYPE_POWERBI;
        public const string STORAGE = ProjectResourceConstants.SERVICE_TYPE_STORAGE;
        public const string SQLSERVER = ProjectResourceConstants.SERVICE_TYPE_SQL_SERVER;
        public const string POSTGRESQL = ProjectResourceConstants.SERVICE_TYPE_POSTGRES;

        private const string RESOURCE_REQUEST_INPUT_JSON_PREFIX = "ResourceInput";

        public RequestManagementService(
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
            IEmailNotificationService emailNotificationService,
            ISystemNotificationService systemNotificationService, 
            IUserInformationService userInformationService,
            IDatahubAuditingService datahubAuditingService, 
            IMiscStorageService miscStorageService)
        {
            _dbContextFactory = dbContextFactory;
            _emailNotificationService = emailNotificationService;
            _systemNotificationService = systemNotificationService;
            _userInformationService = userInformationService;
            _datahubAuditingService = datahubAuditingService;
            _miscStorageService = miscStorageService;
        }

        public async Task RequestAccess(Datahub_Project_Access_Request request)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            ctx.Projects.Attach(request.Project);

            var exists = await ctx.Access_Requests
                .AnyAsync(a => a.User_ID == request.User_ID
                               && a.Project == request.Project
                               && a.Databricks == request.Databricks
                               && a.PowerBI == request.PowerBI
                               && a.WebForms == request.WebForms
                );

            if (!exists)
            {
                await ctx.Access_Requests.AddAsync(request);
            }
        
            await ctx.SaveChangesAsync();
            await NotifyProjectAdminsOfAccessRequest(request);
        }

        public async Task RequestService(Datahub_ProjectServiceRequests request, Dictionary<string, string> inputParams = null)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            ctx.Projects.Attach(request.Project);

            var exists = await ctx.Project_Requests
                .AnyAsync(a => a.Project == request.Project && a.ServiceType == request.ServiceType);

            if (!exists)
            {
                await ctx.Project_Requests.AddAsync(request);

                var projectResource = CreateEmptyProjectResource(request, inputParams);
                await ctx.Project_Resources2.AddAsync(projectResource);

                await ctx.TrackSaveChangesAsync(_datahubAuditingService);
            }

            await NotifyProjectAdminsOfServiceRequest(request);
        }

        private static Project_Resources2 CreateEmptyProjectResource(Datahub_ProjectServiceRequests request, Dictionary<string, string> inputParams)
        {
            var resource = new Project_Resources2()
            {
                Project = request.Project,
                ResourceType = request.ServiceType,
                TimeRequested = DateTime.UtcNow
            };

            if (inputParams != null)
            {
                resource.SetInputParameters(inputParams);
            }

            switch (request.ServiceType)
            {
                case POSTGRESQL:
                case SQLSERVER:
                    resource.SetResourceObject(default(ProjectResource_Database));
                    break;
                case STORAGE:
                    resource.SetResourceObject(default(ProjectResource_Storage));
                    break;
                default:
                    resource.SetResourceObject(default(ProjectResource_Blank));
                    break;
            }

            return resource;

        }

        public async Task<List<Project_Resources2>> GetResourcesByRequest(Datahub_ProjectServiceRequests request)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();

            var resources = await ctx.Project_Resources2
                .Where(r => r.Project == request.Project && r.ResourceType == request.ServiceType)
                .ToListAsync();

            return resources;
        }

        public async Task<bool> UpdateResourceInputParameters(Guid resourceId, Dictionary<string,string> inputParams)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var resource = await ctx.Project_Resources2.FirstOrDefaultAsync(r => r.ResourceId == resourceId);
            if (resource != null)
            {
                resource.SetInputParameters(inputParams);
                await ctx.TrackSaveChangesAsync(_datahubAuditingService);
                return true;
            }
            else
            {
                return false;
            }
        }

        private string GetResourceInputDefinitionIdentifier(string resourceType) => $"{RESOURCE_REQUEST_INPUT_JSON_PREFIX}-{resourceType}";

        public async Task<string> GetResourceInputDefinitionJson(string resourceType)
        {
            var id = GetResourceInputDefinitionIdentifier(resourceType);
            var result = await _miscStorageService.GetObject<string>(id);
            return result;
        }

        public async Task SaveResourceInputDefinitionJson(string resourceType, string jsonContent)
        {
            var id = GetResourceInputDefinitionIdentifier(resourceType);
            await _miscStorageService.SaveObject(jsonContent, id);
        }

        private async Task NotifyProjectAdminsOfServiceRequest(Datahub_ProjectServiceRequests request)
        {
            
            var admins = await GetProjectAdministratorEmailsAndIds(request.Project.Project_ID);
            
            await _emailNotificationService.SendServiceCreationRequestNotification(request.User_Name, request.ServiceType, request.Project.ProjectInfo, admins);
            
            var adminUserIds = admins.
                Where(a => Guid.TryParse(a, out _))
                .ToList();
            var user = await _userInformationService.GetUserAsync();
            
            await _systemNotificationService.CreateSystemNotificationsWithLink(adminUserIds, $"/administration", "SYSTEM-NOTIFICATION.GoToAdminPage",
                "SYSTEM-NOTIFICATION.NOTIFICATION-TEXT.ServiceCreationRequested",
                user.UserPrincipalName, request.ServiceType, new BilingualStringArgument(request.Project.ProjectInfo.ProjectNameEn, request.Project.ProjectInfo.ProjectNameFr));

        }
        
        private async Task NotifyProjectAdminsOfAccessRequest(Datahub_Project_Access_Request request)
        {
            var serviceName = request.RequestServiceType;

            var admins = await GetProjectAdministratorEmailsAndIds(request.Project.Project_ID);
            await _emailNotificationService.SendServiceAccessRequestNotification(request.User_Name, serviceName,
                request.Project.ProjectInfo, admins);
            
            var adminUserIds = admins
                .Where(a => Guid.TryParse(a, out _))
                .ToList();
            
            var user = await _userInformationService.GetUserAsync();

            await _systemNotificationService.CreateSystemNotificationsWithLink(adminUserIds, $"/administration",
                "SYSTEM-NOTIFICATION.GoToAdminPage",
                "SYSTEM-NOTIFICATION.NOTIFICATION-TEXT.ServiceAccessRequested",
                user.UserPrincipalName, serviceName,
                new BilingualStringArgument(request.Project.ProjectInfo.ProjectNameEn, request.Project.ProjectInfo.ProjectNameFr));
        }
        
        private async Task<List<string>> GetProjectAdministratorEmailsAndIds(int projectId)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();

            var project = await ctx.Projects
                .Include(p => p.Users)
                .FirstAsync(p => p.Project_ID == projectId);
            

            var adminEmails = ServiceAuthManager.ExtractEmails(project.Project_Admin ?? string.Empty);

            var adminUsers = project.Users
                .Where(u => u.IsAdmin)
                .Select(u => u.User_ID);

            return adminEmails
                .Concat(adminUsers)
                .ToList();
        }

    }
}
