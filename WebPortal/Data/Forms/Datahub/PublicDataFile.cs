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
        public string RequestingUserEmail_TXT { get; set; }
        
        [Required]
        [StringLength(200)]
        public string RequestingUser_ID { get; set; }

        public DateTime RequestedDate_DT { get; set; }

        //TODO add field(s) for approver (user or person) information
        public DateTime? ApprovedDate_DT { get; set; }
    }
}