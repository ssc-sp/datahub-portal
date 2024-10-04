using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services
{
    public class ProjectResourcingWhitelistService : IProjectResourceWhitelistService
    {

        private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
        private readonly IUserInformationService _userInformationService;
        private readonly IDatahubAuditingService _auditingService;

        public ProjectResourcingWhitelistService(IDbContextFactory<DatahubProjectDBContext> contextFactory,
            IUserInformationService userInformationService,
            IDatahubAuditingService auditingService)
        {
            _contextFactory = contextFactory;
            _userInformationService = userInformationService;
            _auditingService = auditingService;
        }

        public async Task<IEnumerable<Project_Whitelist>> GetAllProjectResourceWhitelistAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var tasks = context.Projects.Select(GetProjectResourceWhitelistByProjectAsync);
            var results = await Task.WhenAll(tasks);

            return results.ToList();
        }

        public async Task<Project_Whitelist> GetWhitelistByProjectAsync(int projectId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var whitelist = await context.Project_Whitelists.FirstOrDefaultAsync(x => x.Project.Project_ID == projectId);
            return whitelist ?? new Project_Whitelist() { ProjectId = projectId };
        }

        public async Task<Project_Whitelist> GetProjectResourceWhitelistByProjectAsync(int projectId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var whitelist = await context.Project_Whitelists
                .Include(wl => wl.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Project.Project_ID == projectId);
            return whitelist ?? new Project_Whitelist() { ProjectId = projectId };
        }

        private async Task<Project_Whitelist> GetProjectResourceWhitelistByProjectAsync(
            Datahub_Project project)
        {
            var whitelist = await GetProjectResourceWhitelistByProjectAsync(project.Project_ID);
            whitelist.Project = project;
            return whitelist;
        }

        public async Task UpdateProjectResourceWhitelistAsync(Project_Whitelist projectResourceWhitelist)
        {
            var currentUser = await _userInformationService.GetCurrentGraphUserAsync();
            
            projectResourceWhitelist.LastUpdated = DateTime.Now;
            projectResourceWhitelist.AdminLastUpdated_ID = currentUser.Id;
            projectResourceWhitelist.AdminLastUpdated_UserName = currentUser.Mail;

            await using var context = await _contextFactory.CreateDbContextAsync();
            
            // add or update whitelist
            if (projectResourceWhitelist.Id == 0)
            {
                context.Project_Whitelists.Add(projectResourceWhitelist);
            }
            else
            {
                context.Project_Whitelists.Update(projectResourceWhitelist);
            }

            await context.TrackSaveChangesAsync(_auditingService);
        }
    }
}