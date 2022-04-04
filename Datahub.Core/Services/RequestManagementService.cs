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

        public const string DATABRICKS = "databricks";
        public const string POWERBI = "powerbi";
        public const string STORAGE = "storage";
        public const string SQLSERVER = "sql";
        public const string POSTGRESQL = "psql";
        


        public RequestManagementService(
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory, 
            IEmailNotificationService emailNotificationService, 
            ISystemNotificationService systemNotificationService, IUserInformationService userInformationService)
        {
            _dbContextFactory = dbContextFactory;
            _emailNotificationService = emailNotificationService;
            _systemNotificationService = systemNotificationService;
            _userInformationService = userInformationService;
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

        public async Task RequestService(Datahub_ProjectServiceRequests request)
        {
            await using var ctx = await _dbContextFactory.CreateDbContextAsync();
            ctx.Projects.Attach(request.Project);

            var exists = await ctx.Project_Requests
                .AnyAsync(a => a.Project == request.Project && a.ServiceType == request.ServiceType);

            if (!exists)
            {
                await ctx.Project_Requests.AddAsync(request);
                await ctx.SaveChangesAsync();
                
                var projectResource = CreateProjectResource(request);
                await ctx.Project_Resources.AddAsync(projectResource);
                await ctx.SaveChangesAsync();
            }
            
            
            
            await NotifyProjectAdminsOfServiceRequest(request);
        }
        
        private static Project_Resources CreateProjectResource(Datahub_ProjectServiceRequests request)
        {
            var attributes = request.ServiceType switch
            {
                "sql" => "\"type\":\"sql\"",
                "psql" => "\"type\":\"psql\"",
                "storage" => "\"type\":\"gen2\"",
                _ => $"\"type\":\"{request.ServiceType}\""
            };
        
            return new Project_Resources      
            {
                ResourceType = request.ServiceType,
                ResourceName = request.Project.Project_Acronym_CD,
                TimeRequested = DateTime.Now,
                Attributes = attributes,
                Project = request.Project
            };
        }
        
        private async Task NotifyProjectAdminsOfServiceRequest(Datahub_ProjectServiceRequests request)
        {
            
            var admins = await GetProjectAdministratorEmailsAndIds(request.Project.Project_ID);
            
            await _emailNotificationService.SendServiceCreationRequestNotification(request.User_Name, request.ServiceType, request.Project.ProjectInfo, admins);
            
            var adminUserIds = admins.
                Where(a => Guid.TryParse(a, out _))
                .ToList();
            var user = await _userInformationService.GetUserAsync();
            
            await _systemNotificationService.CreateSystemNotificationsWithLink(adminUserIds, $"/admin", "SYSTEM-NOTIFICATION.GoToAdminPage",
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

            await _systemNotificationService.CreateSystemNotificationsWithLink(adminUserIds, $"/admin",
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
