using Datahub.Application.Commands;
using Datahub.Application.Services;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Offline;

public class OfflineProjectUserManagementService : IProjectUserManagementService
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;

    public OfflineProjectUserManagementService(IDbContextFactory<DatahubProjectDBContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public Task<bool> ProcessProjectUserCommandsAsync(List<ProjectUserUpdateCommand> projectUserUpdateCommands, List<ProjectUserAddEntraUserCommand> projectUserAddUserCommands)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ProcessProjectUserCommandsAsync(List<ProjectUserUpdateCommand> projectUserUpdateCommands, List<ProjectUserAddEntraUserCommand> projectUserAddUserCommands,
        string requesterUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<UserRoleLinks>> GetProjectUsersAsync(string projectAcronym)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UserRolesLinks
            .AsNoTracking()
            .Include(u => u.Project)
            .Include(u => u.PortalUser)
            .Include(u => u.Role)
            .Where(u => u.Project.Project_Acronym_CD == projectAcronym)
            .Where(u => u.PortalUser != null)
            .ToListAsync();
    }

    public async Task<List<string>> GetProjectListForPortalUser(int portalUserId)
    {
        using (var context = await _contextFactory.CreateDbContextAsync())
        {
            var projectAcronyms = await (from p in context.Projects
                                         join pu in context.UserRolesLinks on p.Project_ID equals pu.Project_ID
                                         where pu.PortalUserId == portalUserId
                                         select p.Project_Acronym_CD).ToListAsync();

            return projectAcronyms;
        }
    }

    public async Task<UserRoleLinks?> GetProjectLeadAsync(string projectAcronym)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var users = await GetProjectUsersAsync(projectAcronym);
        return users?.FirstOrDefault(u => RoleConstants.GetRoleSuffixes(u.Role).Contains(RoleConstants.WORKSPACE_LEAD_SUFFIX));
    }

    public async Task<bool> RunWorkspaceSync(string projectAcronym)
    {
        return false; // cannot run sync while offline
    }
}
