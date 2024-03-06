using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.Model.Datahub;

public class PublicDataFile
{
    [Key]
    public long PublicDataFileID { get; set; }

    public Guid FileID { get; set; }

    [Required]
    [StringLength(256)]
    public string FilenameTXT { get; set; }

    [StringLength(1024)]
    public string FolderPathTXT { get; set; }

    [StringLength(10)]
    public string ProjectCodeCD { get; set; }

    public bool IsProjectBased => ProjectCodeCD != null;

    [Required]
    [StringLength(200)]
    public string RequestingUserID { get; set; }

    // Initial request timestamp
    public DateTime RequestedDateDT { get; set; }

    // Submission timestamp - after filling in required metadata
    public DateTime? SubmittedDateDT { get; set; }

    //TODO add field(s) for approver (user or person) information

    // Approval timestamp
    public DateTime? ApprovedDateDT { get; set; }

    // Publication may be immediately after approval, or specified for a future date
    public DateTime? PublicationDateDT { get; set; }
}