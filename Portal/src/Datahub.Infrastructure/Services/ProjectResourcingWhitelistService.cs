using System.Transactions;
using Datahub.Application.Services;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services
{
    public class ProjectResourcingWhitelistService : IProjectResourceWhitelistService
    {
        
        private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
        private readonly IUserInformationService _userInformationService;
        
        public ProjectResourcingWhitelistService(IDbContextFactory<DatahubProjectDBContext> contextFactory, 
            IUserInformationService userInformationService)
        {
            _contextFactory = contextFactory;
            _userInformationService = userInformationService;
        }

        public async Task<IEnumerable<Datahub_Project_Resources_Whitelist>> GetAllProjectResourceWhitelistAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var tasks = context.Projects.Select(GetProjectResourceWhitelistByProjectAsync);
            var results = await Task.WhenAll(tasks);

            return results.ToList();
        }

        public async Task<Datahub_Project_Resources_Whitelist> GetProjectResourceWhitelistByProjectAsync(int projectId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var whitelist = await context.Project_Resources_Whitelists
                .Include(wl => wl.Project)
                .FirstOrDefaultAsync(x => x.Project.Project_ID == projectId);
            return whitelist ?? new Datahub_Project_Resources_Whitelist();
        }

        private async Task<Datahub_Project_Resources_Whitelist> GetProjectResourceWhitelistByProjectAsync(
            Datahub_Project project)
        {
            var whitelist = await GetProjectResourceWhitelistByProjectAsync(project.Project_ID);
            whitelist.Project = project;
            return whitelist;
        }

        public async Task UpdateProjectResourceWhitelistAsync(Datahub_Project_Resources_Whitelist projectResourceWhitelist)
        {
            using var scope = new TransactionScope(
                TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
            var currentUser = await _userInformationService.GetCurrentGraphUserAsync();
            projectResourceWhitelist.LastUpdated = DateTime.Now;
            projectResourceWhitelist.AdminLastUpdated_ID = currentUser.Id;
            projectResourceWhitelist.AdminLastUpdated_UserName = currentUser.Mail;
            await using var context = await _contextFactory.CreateDbContextAsync();
            if (projectResourceWhitelist.Id == 0)
            {
                context.Project_Resources_Whitelists.Add(projectResourceWhitelist);
            }
            else
            {
                context.Project_Resources_Whitelists.Update(projectResourceWhitelist);
            }
            await context.SaveChangesAsync();
            scope.Complete();
        }
    }
}