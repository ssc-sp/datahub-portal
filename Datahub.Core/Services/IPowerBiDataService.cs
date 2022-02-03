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
        public Task AddCataloguedWorkspace(Guid workspaceId, string workspaceName, bool sandboxFlag, int? projectId);
        public Task UpdateCataloguedWorkspace(Guid workspaceId, string workspaceName, bool sandboxFlag, int? projectId);
    }
}
