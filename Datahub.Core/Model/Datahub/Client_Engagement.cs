using Microsoft.Graph;
using MudBlazor;
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

        [AeFormCategory("Projected Engagement Details")]
        [Required]
        [MaxLength(2000)]
        public string Engagement_Name { get; set; }
        [AeFormIgnore]
        public Datahub_Project Project { get; set; }

        [AeFormCategory("Projected Engagement Details")]
        public bool Is_Engagement_Active { get; set; }

        [AeFormCategory("Projected Engagement Details")]
        public DateTime? Engagement_Start_Date { get; set; }
        [AeFormCategory("Projected Engagement Details")]
        public DateTime? Requirements_Gathering_EndDate { get; set; }
        [AeFormCategory("Projected Engagement Details")]
        public DateTime? Phase1_Development_EndDate { get; set; }
        [AeFormCategory("Projected Engagement Details")]
        public DateTime? Phase1_Testing_EndDate { get; set; }
        [AeFormCategory("Projected Engagement Details")]
        public DateTime? Phase2_Development_EndDate { get; set; }
        [AeFormCategory("Projected Engagement Details")]
        public DateTime? Phase2_Testing_EndDate { get; set; }
        [AeFormCategory("Projected Engagement Details")]
        public DateTime? Final_Updates_EndDate { get; set; }
        [AeFormCategory("Projected Engagement Details")]
        public DateTime? Final_Release_Date { get; set; }
        [AeFormCategory("Actual Engagement Details")]
        public DateTime? Requirements_Gathering_ActualEndDate { get; set; }
        [AeFormCategory("Actual Engagement Details")]
        public DateTime? Phase1_Development_ActualEndDate { get; set; }
        [AeFormCategory("Actual Engagement Details")]
        public DateTime? Phase1_Testing_ActualEndDate { get; set; }
        [AeFormCategory("Actual Engagement Details")]
        public DateTime? Phase2_Development_ActualEndDate { get; set; }
        [AeFormCategory("Actual Engagement Details")]
        public DateTime? Phase2_Testing_ActualEndDate { get; set; }
        [AeFormCategory("Actual Engagement Details")]
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


        [AeFormIgnore]
        public bool IsRequirementsComplete => Requirements_Gathering_ActualEndDate != null;
        [AeFormIgnore]
        public bool IsPhase1DevComplete => Phase1_Development_ActualEndDate != null;
        [AeFormIgnore]
        public bool IsPhase1TestComplete => Phase1_Testing_ActualEndDate != null;
        [AeFormIgnore]
        public bool IsPhase2DevComplete => Phase2_Development_ActualEndDate != null;
        [AeFormIgnore]
        public bool IsPhase2TestComplete => Phase2_Testing_ActualEndDate != null;
        [AeFormIgnore]
        public bool IsReleased => Actual_Release_Date != null;

        public (Color, Severity) RequirementsStatus()
        {
            if (!IsRequirementsComplete)
            {
                if (Requirements_Gathering_EndDate is null)
                {
                    return (Color.Info, Severity.Info);
                }
                else
                {
                    return Requirements_Gathering_EndDate <= DateTime.Now.Date ? (Color.Success, Severity.Success) : (Color.Error, Severity.Error);
                }
            }
            else
            {
                return Requirements_Gathering_EndDate <= Requirements_Gathering_ActualEndDate ? (Color.Success, Severity.Success) : (Color.Error, Severity.Error);
            }
        }

        public (Color, Severity) TimelineStatus(bool isComplete, DateTime? estimatedEndDate, DateTime? actualEndDate)
        {
            if (!isComplete)
            {
                if (estimatedEndDate is null)
                {
                    return (Color.Info, Severity.Info);
                }
                else
                {
                    return estimatedEndDate >= DateTime.Now.Date ? (Color.Success, Severity.Success) : (Color.Error, Severity.Error);
                }
            }
            else
            {
                return estimatedEndDate >= actualEndDate ? (Color.Success, Severity.Success) : (Color.Error, Severity.Error);
            }
        }
    }
}
