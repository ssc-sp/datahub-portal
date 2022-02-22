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
        private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
        private readonly ILogger<PowerBiDataService> _logger;

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

        public async Task<bool> AddOrUpdateCataloguedWorkspaces(IEnumerable<PowerBi_WorkspaceDefinition> workspaceDefinitions)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();

            return await DoUpdateWorkspaces(ctx, workspaceDefinitions);
        }

        private async Task<bool> DoUpdateWorkspaces(DatahubProjectDBContext ctx, IEnumerable<PowerBi_WorkspaceDefinition> workspaceDefinitions)
        {
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

            try
            {
                await ctx.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating PowerBI workspaces");
                return false;
            }
        }

        public async Task<IList<PowerBi_DataSet>> GetAllDatasets()
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            var results = await ctx.PowerBi_DataSets.ToListAsync();
            return results;
        }

        public async Task<bool> AddOrUpdateCataloguedDatasets(IEnumerable<PowerBi_DataSetDefinition> datasetDefinitions)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();

            return await DoUpdateDatasets(ctx, datasetDefinitions);
        }

        private async Task<bool> DoUpdateDatasets(DatahubProjectDBContext ctx, IEnumerable<PowerBi_DataSetDefinition> datasetDefinitions)
        {
            foreach (var def in datasetDefinitions)
            {
                var dataset = await ctx.PowerBi_DataSets.FirstOrDefaultAsync(d => d.DataSet_ID == def.DataSetId);
                if (dataset == null)
                {
                    _logger.LogDebug("Creating dataset record for {} ({}) in workspace {}", def.DataSetName, def.DataSetId, def.WorkspaceId);

                    dataset = new()
                    {
                        DataSet_ID = def.DataSetId,
                        DataSet_Name = def.DataSetName,
                        Workspace_Id = def.WorkspaceId
                    };

                    ctx.PowerBi_DataSets.Add(dataset);
                }
                else
                {
                    _logger.LogDebug("Updating dataset {} ({})", def.DataSetName, def.DataSetId);

                    dataset.DataSet_ID = def.DataSetId;
                    dataset.DataSet_Name = def.DataSetName;
                    dataset.Workspace_Id = def.WorkspaceId;
                }
            }

            try
            {
                await ctx.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating PowerBI datasets");
                return false;
            }
        }

        public async Task<IList<PowerBi_Report>> GetAllReports()
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            var reports = await ctx.PowerBi_Reports.ToListAsync();
            return reports;
        }

        public async Task<bool> AddOrUpdateCataloguedReports(IEnumerable<PowerBi_ReportDefinition> reportDefinitions)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();

            return await DoUpdateReports(ctx, reportDefinitions);
        }

        private async Task<bool> DoUpdateReports(DatahubProjectDBContext ctx, IEnumerable<PowerBi_ReportDefinition> reportDefinitions)
        {
            foreach (var def in reportDefinitions)
            {
                var report = await ctx.PowerBi_Reports.FirstOrDefaultAsync(d => d.Report_ID == def.ReportId);
                if (report == null)
                {
                    _logger.LogDebug("Creating report record for {} ({}) in workspace {}", def.ReportName, def.ReportId, def.WorkspaceId);

                    report = new()
                    {
                        Report_ID = def.ReportId,
                        Report_Name = def.ReportName,
                        Workspace_Id = def.WorkspaceId
                    };

                    ctx.PowerBi_Reports.Add(report);
                }
                else
                {
                    _logger.LogDebug("Updating report {} ({})", def.ReportName, def.ReportId);

                    report.Report_ID = def.ReportId;
                    report.Report_Name = def.ReportName;
                    report.Workspace_Id = def.WorkspaceId;
                }
            }

            try
            {
                await ctx.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating PowerBI reports");
                return false;
            }
        }

        public async Task<List<PowerBi_Report>> GetReportsForProject(string projectCode)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();

            var results = await ctx.PowerBi_Reports
                .Include(r => r.Workspace)
                .ThenInclude(w => w.Project)
                .Where(r => r.Workspace.Project != null && r.Workspace.Project.Project_Acronym_CD.ToLower() == projectCode.ToLower())
                .ToListAsync();
            return results;
        }

        public async Task<List<PowerBi_Report>> GetReportsForUser(string userId)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();

            var results = await ctx.PowerBi_Reports
                .Include(r => r.Workspace)
                .ThenInclude(w => w.Project)
                .ThenInclude(p => p.Users)
                .Where(r => r.Workspace.Project != null && r.Workspace.Project.Users.Any(u => u.User_ID == userId))
                .Distinct()
                .ToListAsync();

            return results;
        }

        public async Task<bool> BulkAddOrUpdatePowerBiItems(IEnumerable<PowerBi_WorkspaceDefinition> workspaceDefinitions, 
            IEnumerable<PowerBi_DataSetDefinition> datasetDefinitions, 
            IEnumerable<PowerBi_ReportDefinition> reportDefinitions)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            var transaction = await ctx.Database.BeginTransactionAsync();

            var success = true;

            success &= await DoUpdateWorkspaces(ctx, workspaceDefinitions);
            success &= await DoUpdateDatasets(ctx, datasetDefinitions);
            success &= await DoUpdateReports(ctx, reportDefinitions);

            if (success)
            {
                await transaction.CommitAsync();
            }
            else
            {
                await transaction.RollbackAsync();
            }

            return success;
        }
    }
}
