using Datahub.Core.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class PowerBiDataService : IPowerBiDataService
    {
        private IDbContextFactory<DatahubProjectDBContext> _contextFactory;
        private ILogger<PowerBiDataService> _logger;

        public PowerBiDataService(
            IDbContextFactory<DatahubProjectDBContext> contextFactory,
            ILogger<PowerBiDataService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<IList<PowerBi_Workspace>> GetAllWorkspaces()
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            var results = await ctx.PowerBi_Workspaces.Include(w => w.Project).ToListAsync();
            return results;
        }

        public async Task AddOrUpdateCataloguedWorkspaces(IEnumerable<PowerBi_WorkspaceDefinition> workspaceDefinitions)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();

            foreach (var def in workspaceDefinitions)
            {
                var workspace = await ctx.PowerBi_Workspaces.FirstOrDefaultAsync(w => w.Workspace_ID == def.WorkspaceId);
                if (workspace == null)
                {
                    _logger.LogDebug("Creating workspace record for {} ({})", def.WorkspaceName, def.WorkspaceId);

                    workspace = new PowerBi_Workspace()
                    {
                        Workspace_ID = def.WorkspaceId,
                        Workspace_Name = def.WorkspaceName,
                        Sandbox_Flag = def.SandboxFlag,
                        Project_Id = def.ProjectId
                    };

                    ctx.PowerBi_Workspaces.Add(workspace);
                }
                else
                {
                    _logger.LogDebug("Updating workspace record for {} ({})", def.WorkspaceName, def.WorkspaceId);

                    workspace.Workspace_Name = def.WorkspaceName;
                    workspace.Sandbox_Flag = def.SandboxFlag;
                    workspace.Project_Id = def.ProjectId;
                }
            }

            await ctx.SaveChangesAsync();
        }

        public async Task AddCataloguedWorkspace(Guid workspaceId, string workspaceName, bool sandboxFlag, int? projectId)
        {
            var workspace = new PowerBi_Workspace()
            {
                Workspace_ID = workspaceId,
                Project_Id = projectId,
                Sandbox_Flag = sandboxFlag,
                Workspace_Name = workspaceName
            };

            using var ctx = await _contextFactory.CreateDbContextAsync();
            ctx.PowerBi_Workspaces.Add(workspace);
            await ctx.SaveChangesAsync();
        }

        public async Task UpdateCataloguedWorkspace(Guid workspaceId, string workspaceName, bool sandboxFlag, int? projectId)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();

            var existingWorkspace = await ctx.PowerBi_Workspaces.FirstOrDefaultAsync(w => w.Workspace_ID == workspaceId);
            if (existingWorkspace != null)
            {
                existingWorkspace.Workspace_Name = workspaceName;
                existingWorkspace.Sandbox_Flag = sandboxFlag;
                existingWorkspace.Project_Id = projectId;

                await ctx.SaveChangesAsync();
            }
        }
    }
}
