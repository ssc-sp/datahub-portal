using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRCan.Datahub.Shared.EFCore
{
    public class SharedDataFile
    {
        [Key]
        public long SharedDataFile_ID { get; set; }

        public Guid File_ID { get; set; }

        public bool IsOpenDataRequest_FLAG { get; set; } = false;

        public string Filename_TXT { get; set; }
        
        public string FolderPath_TXT { get; set; }
        public string ProjectCode_CD { get; set; }
        public bool IsProjectBased => !string.IsNullOrEmpty(ProjectCode_CD);

        [Required]
        [StringLength(200)]
        public string RequestingUser_ID { get; set; }
        
        [StringLength(200)]
        public string ApprovingUser_ID { get; set; }

        public DateTime RequestedDate_DT { get; set; }
        public DateTime? SubmittedDate_DT { get; set; }
        public DateTime? ApprovedDate_DT { get; set; }
        public DateTime? PublicationDate_DT { get; set; }
    }

    [Table("OpenDataSharedFile")]
    public class OpenDataSharedFile: SharedDataFile
    {
        public int? ApprovalForm_ID { get; set; }
        public string SignedApprovalForm_URL { get; set; }
    }
}