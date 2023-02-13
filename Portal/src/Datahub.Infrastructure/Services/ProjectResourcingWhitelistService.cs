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
            // await using var context = await _contextFactory.CreateDbContextAsync();
                                         // return context.Projects.Select(async p => await GetProjectResourceWhitelistByProjectAsync(p.Project_ID))
                                         //     .Select(t => t.Result)
                                         //     .ToList();
                                         
            return new List<Datahub_Project_Resources_Whitelist>();
        }

        public async Task<Datahub_Project_Resources_Whitelist> GetProjectResourceWhitelistByProjectAsync(int projectId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var whitelist = await context.Project_Resources_Whitelists
                .Include(wl => wl.Project)
                .FirstOrDefaultAsync(x => x.Project.Project_ID == projectId);
            return whitelist ?? new Datahub_Project_Resources_Whitelist();
        }

        public async Task UpdateProjectResourceWhitelistAsync(Datahub_Project_Resources_Whitelist projectResourceWhitelist)
        {
            var currentUser = await _userInformationService.GetCurrentGraphUserAsync();
            projectResourceWhitelist.LastUpdated = DateTime.Now;
            projectResourceWhitelist.AdminLastUpdated_ID = currentUser.Id;
            projectResourceWhitelist.AdminLastUpdated_UserName = currentUser.Mail;
            await using var context = await _contextFactory.CreateDbContextAsync();
            if (projectResourceWhitelist.Id == 0)
            {
                await context.Project_Resources_Whitelists.AddAsync(projectResourceWhitelist);
            }
            else
            {
                context.Project_Resources_Whitelists.Update(projectResourceWhitelist);
            }
            await context.SaveChangesAsync();
        }
    }
}