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
        public Task<IList<PowerBi_DataSet>> GetAllDatasets();
        public Task<bool> AddOrUpdateCataloguedDatasets(IEnumerable<PowerBi_DataSetDefinition> datasetDefinitions);
        public Task<IList<PowerBi_Report>> GetAllReports();
        public Task<bool> AddOrUpdateCataloguedReports(IEnumerable<PowerBi_ReportDefinition> reportDefinitions);
    }
}
