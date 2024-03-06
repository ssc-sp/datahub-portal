using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.Model.Projects;

public class DatahubProjectUserRequest
{
    [Key]
    public int ProjectUserRequestID { get; set; }

    [StringLength(200)]
    public string UserID { get; set; }

    public DateTime RequestedDT { get; set; }

    public DateTime? ApprovedDT { get; set; }

    public string ApprovedUser { get; set; }

    public DatahubProject Project { get; set; }

    [Timestamp]
    public byte[] Timestamp { get; set; }
}