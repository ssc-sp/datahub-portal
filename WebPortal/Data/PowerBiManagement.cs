using Datahub.Core.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.Portal.Data
{
    public class PowerBiAdminWorkspaceItem
    {
        public Guid WorkspaceId { get; set; }
        public string WorkspaceName { get; set; }
        public bool SandboxFlag { get; set; }
        public PowerBi_Workspace WorkspaceFromDb { get; set; }
        public bool IsCatalogued => WorkspaceFromDb != null;
        public string ProjectName => WorkspaceFromDb?.Project?.ProjectName;
        public int? ProjectId { get; set; }
        public int ProjectIdDropdown
        {
            get => ProjectId ?? -1;
            set
            {
                ProjectId = (value < 0) ? null : value;
            }
        }
    }
}
