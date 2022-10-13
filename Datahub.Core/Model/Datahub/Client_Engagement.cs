using MudBlazor.Forms;
using System;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.EFCore
{
    public class Client_Engagement
    {
        [AeFormIgnore]
        [Key]
        public int Engagement_ID { get; set; }

        [AeFormCategory("Engagement Details")]
        [Required]
        [MaxLength(2000)]
        public string Engagement_Name { get; set; }
        [AeFormIgnore]
        public Datahub_Project Project { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Engagement_Start_Date { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Requirements_Gathering_EndDate { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Requirements_Gathering_ActualEndDate { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Phase1_Development_EndDate { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Phase1_Development_ActualEndDate { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Phase1_Testing_EndDate { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Phase1_Testing_ActualEndDate { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Phase2_Development_EndDate { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Phase2_Development_ActualEndDate { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Phase2_Testing_EndDate { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Phase2_Testing_ActualEndDate { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Final_Updates_EndDate { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Final_Release_Date { get; set; }
        [AeFormCategory("Engagement Details")]
        public DateTime? Actual_Release_Date { get; set; }

        [AeFormIgnore]
        public string Last_Updated_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        public string Created_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Created_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }

    }
}
