using System.ComponentModel.DataAnnotations;
using MudBlazor.Forms;

namespace Datahub.Core.Model.Projects;

public class Datahub_ProjectComment
{
    [Key]
    [AeFormIgnore]
    public int Comment_ID { get; set; }

    public DateTime Comment_Date_DT { get; set; }

    public string Comment_NT { get; set; }

    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }
    public Datahub_Project Project { get; set; }

    [AeFormIgnore]
    public string Last_Updated_UserId { get; set; }

    [AeFormIgnore]
    public DateTime Last_Updated_DT { get; set; }

    [AeFormIgnore]
    public string Created_UserId { get; set; }

    [AeFormIgnore]
    public DateTime Created_DT { get; set; }
}