using System.ComponentModel.DataAnnotations.Schema;

namespace Datahub.Core.Model.Projects;

public class DatahubProjectPipelineLnk
{
    public int ProjectID { get; set; }

    [ForeignKey("Project_ID")]
    public DatahubProject Project { get; set; }
    public string ProcessNm { get; set; }
}