using System.ComponentModel.DataAnnotations;
using MudBlazor.Forms;

namespace Datahub.Core.Model.Projects;

public class DatahubProjectComment
{
    [Key]
    [AeFormIgnore]
    public int CommentID { get; set; }

    public DateTime CommentDateDT { get; set; }

    public string CommentNT { get; set; }

    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }
    public DatahubProject Project { get; set; }

    [AeFormIgnore]
    public string LastUpdatedUserId { get; set; }

    [AeFormIgnore]
    public DateTime LastUpdatedDT { get; set; }

    [AeFormIgnore]
    public string CreatedUserId { get; set; }

    [AeFormIgnore]
    public DateTime CreatedDT { get; set; }
}