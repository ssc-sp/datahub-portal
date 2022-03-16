using Datahub.Core.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public interface IPowerBiDataService
    {
        public Task<IList<PowerBi_Workspace>> GetAllWorkspaces();
        public Task<bool> AddOrUpdateCataloguedWorkspaces(IEnumerable<PowerBi_WorkspaceDefinition> workspaceDefinitions);
        public Task<bool> AddOrUpdateCataloguedWorkspace(PowerBi_WorkspaceDefinition def) => AddOrUpdateCataloguedWorkspaces(new List<PowerBi_WorkspaceDefinition> { def });
        public Task<PowerBi_Workspace> GetWorkspaceById(Guid id);
        public Task<IList<PowerBi_DataSet>> GetAllDatasets();
        public Task<bool> AddOrUpdateCataloguedDatasets(IEnumerable<PowerBi_DataSetDefinition> datasetDefinitions);
        public Task<PowerBi_DataSet> GetDatasetById(Guid id);
        public Task<IList<PowerBi_Report>> GetAllReports();
        public Task<bool> AddOrUpdateCataloguedReports(IEnumerable<PowerBi_ReportDefinition> reportDefinitions);
        public Task<PowerBi_Report> GetReportById(Guid id);
        public Task<bool> BulkAddOrUpdatePowerBiItems(IEnumerable<PowerBi_WorkspaceDefinition> workspaceDefinitions, IEnumerable<PowerBi_DataSetDefinition> datasetDefinitions, IEnumerable<PowerBi_ReportDefinition> reportDefinitions);

        public Task<List<PowerBi_Report>> GetReportsForProject(string projectCode, bool includeSandbox = false);
        public Task<List<PowerBi_Report>> GetReportsForUser(string userId);
    }
}
