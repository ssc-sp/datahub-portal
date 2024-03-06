using Datahub.Application.Services;
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

        public async Task<IEnumerable<ProjectWhitelist>> GetAllProjectResourceWhitelistAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var tasks = context.Projects.Select(GetProjectResourceWhitelistByProjectAsync);
            var results = await Task.WhenAll(tasks);

            return results.ToList();
        }

        public async Task<ProjectWhitelist> GetWhitelistByProjectAsync(int projectId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var whitelist = await context.ProjectWhitelists.FirstOrDefaultAsync(x => x.Project.ProjectID == projectId);
            return whitelist ?? new ProjectWhitelist() { ProjectId = projectId };
        }

        public async Task<ProjectWhitelist> GetProjectResourceWhitelistByProjectAsync(int projectId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var whitelist = await context.ProjectWhitelists
                .Include(wl => wl.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Project.ProjectID == projectId);
            return whitelist ?? new ProjectWhitelist() { ProjectId = projectId };
        }

        private async Task<ProjectWhitelist> GetProjectResourceWhitelistByProjectAsync(
            DatahubProject project)
        {
            var whitelist = await GetProjectResourceWhitelistByProjectAsync(project.ProjectID);
            whitelist.Project = project;
            return whitelist;
        }

        public async Task UpdateProjectResourceWhitelistAsync(ProjectWhitelist projectResourceWhitelist)
        {
            var currentUser = await _userInformationService.GetCurrentGraphUserAsync();

            projectResourceWhitelist.LastUpdated = DateTime.Now;
            projectResourceWhitelist.AdminLastUpdatedID = currentUser.Id;
            projectResourceWhitelist.AdminLastUpdatedUserName = currentUser.Mail;

            await using var context = await _contextFactory.CreateDbContextAsync();

            // add or update whitelist
            if (projectResourceWhitelist.Id == 0)
            {
                context.ProjectWhitelists.Add(projectResourceWhitelist);
            }
            else
            {
                context.ProjectWhitelists.Update(projectResourceWhitelist);
            }

            await context.TrackSaveChangesAsync(_auditingService);
        }
    }
}