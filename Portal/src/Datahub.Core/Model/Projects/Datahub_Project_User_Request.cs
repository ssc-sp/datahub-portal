using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.Model.Projects;

public class Datahub_Project_User_Request
{
    [Key]
    public int ProjectUserRequest_ID { get; set; }

    [StringLength(200)]
    public string User_ID { get; set; }

    public DateTime Requested_DT { get; set; }

    public DateTime? Approved_DT { get; set;  }

    public string ApprovedUser { get; set; }

    public Datahub_Project Project { get; set; }

    [Timestamp]
    public byte[] Timestamp { get; set; }
}