using System.ComponentModel.DataAnnotations.Schema;

namespace Datahub.Core.Model.Projects;

public class Datahub_Project_Pipeline_Lnk
{
    public int Project_ID { get; set; }

    [ForeignKey("Project_ID")]
    public Datahub_Project Project { get; set; }
    public string Process_Nm { get; set; }
}