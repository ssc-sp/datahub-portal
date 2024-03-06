using Datahub.Application.Commands;
using Datahub.Application.Services;
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

    public Task<bool> ProcessProjectUserCommandsAsync(List<ProjectUserUpdateCommand> projectUserUpdateCommands, List<ProjectUserAddUserCommand> projectUserAddUserCommands)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ProcessProjectUserCommandsAsync(List<ProjectUserUpdateCommand> projectUserUpdateCommands, List<ProjectUserAddUserCommand> projectUserAddUserCommands,
        string requesterUserId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<DatahubProjectUser>> GetProjectUsersAsync(string projectAcronym)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ProjectUsers
            .AsNoTracking()
            .Include(u => u.Project)
            .Include(u => u.PortalUser)
            .Include(u => u.Role)
            .Where(u => u.Project.ProjectAcronymCD == projectAcronym)
            .Where(u => u.PortalUser != null)
            .ToListAsync();
    }
}