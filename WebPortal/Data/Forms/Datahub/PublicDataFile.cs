using System;
using System.ComponentModel.DataAnnotations;

namespace NRCan.Datahub.Data.Projects
{
    public class PublicDataFile
    {
        [Key]
        public long PublicDataFile_ID { get; set; }
        
        public Guid File_ID { get; set; }

        [Required]
        [StringLength(256)]
        public string Filename_TXT { get; set; }
        
        [StringLength(1024)]
        public string FolderPath_TXT { get; set; }

        [StringLength(10)]
        public string ProjectCode_CD { get; set; }

        public bool IsProjectBased => ProjectCode_CD != null;
        
        [Required]
        [StringLength(200)]
        public string RequestingUser_ID { get; set; }

        // Initial request timestamp
        public DateTime RequestedDate_DT { get; set; }

        // Submission timestamp - after filling in required metadata
        public DateTime? SubmittedDate_DT { get; set; }

        //TODO add field(s) for approver (user or person) information

        // Approval timestamp
        public DateTime? ApprovedDate_DT { get; set; }

        // Publication may be immediately after approval, or specified for a future date
        public DateTime? PublicationDate_DT { get; set; } 
    }
}