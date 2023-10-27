using System.Linq.Dynamic.Core;
using Datahub.Application.Services.Projects;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services.Projects
{
    public class ProjectInactivityService : IProjectInactivityService
    {
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        
        public ProjectInactivityService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<int> GetDaysSinceLastLogin(int projectId, CancellationToken ct)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync(ct);

            var projectInactivity = await FindProjectInactivity(ctx, projectId, ct);

            return projectInactivity.DaysSinceLastLogin;
        }

        public async Task<int> UpdateDaysSinceLastLogin(int projectId, CancellationToken ct)
        {
            var ctx = await _dbContextFactory.CreateDbContextAsync(ct);
            var projectInactivity = await GetProjectInactivity(ctx, projectId, ct);
            
            // calculate last login
            var lastLogin = await CalculateLastLoginDate(ctx, projectId, ct);
            
            // update last login
            projectInactivity.DaysSinceLastLogin = lastLogin;
            
            // add or update
            await AddOrUpdateProjectInactivity(ctx, projectInactivity, ct);
            
            return lastLogin;
        }

        public async Task<bool> GetProjectWhitelisted(int projectId, CancellationToken ct)
        {
            var ctx = await _dbContextFactory.CreateDbContextAsync(ct);
            var projectInactivity = await FindProjectInactivity(ctx, projectId, ct);
            return projectInactivity.Whitelisted;
        }
        
        public async Task SetProjectWhitelisted(int projectId, bool whitelisted, CancellationToken ct)
        {
            var ctx = await _dbContextFactory.CreateDbContextAsync(ct);
            var projectInactivity = await FindProjectInactivity(ctx, projectId, ct);
            projectInactivity.Whitelisted = whitelisted;
            await AddOrUpdateProjectInactivity(ctx, projectInactivity, ct);
        }

        public async Task<DateTime?> GetProjectRetirementDate(int projectId, CancellationToken ct)
        {
            var ctx = await _dbContextFactory.CreateDbContextAsync(ct);
            var projectInactivity = await FindProjectInactivity(ctx, projectId, ct);
            return projectInactivity.RetirementDate;
        }

        public async Task<int?> GetThresholdNotified(int projectId, CancellationToken ct)
        {
            var ctx = await _dbContextFactory.CreateDbContextAsync(ct);
            var projectInactivity = await FindProjectInactivity(ctx, projectId, ct);
            return projectInactivity.ThresholdNotified;
        }
        
        public async Task<DateTime?> GetDateLastNotified(int projectId, CancellationToken ct)
        {
            var ctx = await _dbContextFactory.CreateDbContextAsync(ct);
            var projectInactivity = await FindProjectInactivity(ctx, projectId, ct);
            return projectInactivity.DateLastNotified;
        }

        public async Task SetProjectRetirementDate(int projectId, DateTime retirementDate, CancellationToken ct)
        {
            var ctx = await _dbContextFactory.CreateDbContextAsync(ct);
            var projectInactivity = await FindProjectInactivity(ctx, projectId, ct);
            projectInactivity.RetirementDate = retirementDate;
            await AddOrUpdateProjectInactivity(ctx, projectInactivity, ct);
        }

        public async Task SetProjectThresholdNotified(int projectId, int thresholdNotified, CancellationToken ct)
        {
            var ctx = await _dbContextFactory.CreateDbContextAsync(ct);
            var projectInactivity = await FindProjectInactivity(ctx, projectId, ct);
            projectInactivity.ThresholdNotified = thresholdNotified;
            await AddOrUpdateProjectInactivity(ctx, projectInactivity, ct);
        }

        public async Task SetProjectDateLastNotified(int projectId, DateTime dateLastNotified, CancellationToken ct)
        {
            var ctx = await _dbContextFactory.CreateDbContextAsync(ct);
            var projectInactivity = await FindProjectInactivity(ctx, projectId, ct);
            projectInactivity.DateLastNotified = dateLastNotified;
            await AddOrUpdateProjectInactivity(ctx, projectInactivity, ct);
        }

        private async Task<Project_Inactivity> GetProjectInactivity(DatahubProjectDBContext ctx, int projectId, CancellationToken ct)
        {
            // find if exists
            var projectInactivity = await ctx.Project_Inactivity.FirstOrDefaultAsync(e => e.ProjectId == projectId, ct);
            
            // create if it does not exist
            projectInactivity ??= new() { ProjectId = projectId, DaysSinceLastLogin = 0, Whitelisted = false, RetirementDate = null, ThresholdNotified = null, DateLastNotified = null};

            return projectInactivity;
        }
        
        private async Task<Project_Inactivity> FindProjectInactivity(DatahubProjectDBContext ctx, int projectId, CancellationToken ct)
        {
            // find if exists
            var projectInactivity = await ctx.Project_Inactivity.FirstOrDefaultAsync(e => e.ProjectId == projectId, ct);
            
            if (projectInactivity is null)
            {
                throw new Exception($"Could not find project with Project_Id: {projectId}");
            }
            
            return projectInactivity;
        }

        private async Task AddOrUpdateProjectInactivity(DatahubProjectDBContext ctx, Project_Inactivity pi, CancellationToken ct)
        {
            if (ctx.Project_Inactivity.Select(x => x.ProjectId).ToList().Contains(pi.ProjectId))
            {
                ctx.Project_Inactivity.Update(pi);
            }
            else
            {
                ctx.Project_Inactivity.Add(pi);
            }
        }

        private async Task<int> CalculateLastLoginDate(DatahubProjectDBContext ctx, int projectId, CancellationToken ct)
        {
            // Get all users in project
            var projectUsers = await ctx.Project_Users.Where(x => x.Project_ID == projectId).Select(x => x.PortalUserId).ToListAsync(ct);
            
            // Find user info
            var projectUsersInfo = await ctx.PortalUsers.Join(
                projectUsers,
                portalUser =>  portalUser.Id ,
                projectUser =>  projectUser ,
                (portalUser, projectUser) => new { portalUser.Id, portalUser.Email, portalUser.LastLoginDateTime}).ToListAsync(ct);
            
            // Calculate most recent login date in terms of days ago
            var lastLogin = projectUsersInfo.Select(x => x.LastLoginDateTime).Max().Value.DayOfYear - DateTime.Now.DayOfYear;
            
            return lastLogin;
        }
    }
}