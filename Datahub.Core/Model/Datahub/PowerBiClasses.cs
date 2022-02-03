using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.EFCore
{
    public class PowerBi_Workspace
    {
        [Key]
        public Guid Workspace_ID { get; set; }

        public string Workspace_Name { get; set; }

        public bool Sandbox_Flag { get; set; }

        public int? Project_Id { get; set; }

        
        public Datahub_Project Project { get; set; }
        
        public IList<PowerBi_Report> Reports { get; set; }
        
        public IList<PowerBi_DataSet> Datasets { get; set; }
    }

    public record class PowerBi_WorkspaceDefinition(Guid WorkspaceId, string WorkspaceName, bool SandboxFlag, int? ProjectId);

    public class PowerBi_Report
    {
        [Key]
        public Guid Report_ID { get; set; }

        public string Report_Name { get; set; }

        public Guid Workspace_Id { get; set; }

        public PowerBi_Workspace Workspace { get; set; }
    }

    public class PowerBi_DataSet
    {
        [Key]
        public Guid DataSet_ID { get; set; }

        public string DataSet_Name { get; set; }

        public Guid Workspace_Id { get; set; }

        public PowerBi_Workspace Workspace { get; set; }
    }
}
