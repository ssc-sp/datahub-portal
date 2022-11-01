using Elemental.Components;
using Microsoft.Graph;
using MudBlazor;
using MudBlazor.Forms;
using System;
using System.ComponentModel.DataAnnotations;
using AeFormCategoryAttribute = MudBlazor.Forms.AeFormCategoryAttribute;
using AeFormIgnoreAttribute = Elemental.Components.AeFormIgnoreAttribute;

namespace Datahub.Core.EFCore
{
    public class Client_Engagement
    {
        [MudBlazor.Forms.AeFormIgnoreAttribute]
        [Key]
        public int Engagement_ID { get; set; }

        [AeFormCategory("Projected Engagement Details")]
        [Required]
        [MaxLength(2000)]
        public string Engagement_Name { get; set; }

        [AeFormCategory("Projected Engagement Details")]
        [Required]
        [MaxLength(2000)]
        public string Engagment_Owners { get; set; }

        [AeFormCategory("Projected Engagement Details")]
        [Required]
        [MaxLength(2000)]
        public string Engagment_Lead{ get; set; }

        [MudBlazor.Forms.AeFormIgnoreAttribute]
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
        [AeLabel(placeholder: "Please Select")]
        public DateTime? Phase2_Development_ActualEndDate { get; set; }
        [AeFormCategory("Actual Engagement Details")]
        public DateTime? Phase2_Testing_ActualEndDate { get; set; }
        [AeFormCategory("Actual Engagement Details")]
        public DateTime? Actual_Release_Date { get; set; }

        [MudBlazor.Forms.AeFormIgnoreAttribute]
        public string Last_Updated_UserId { get; set; }

        [MudBlazor.Forms.AeFormIgnoreAttribute]
        public DateTime Last_Updated_DT { get; set; }

        [MudBlazor.Forms.AeFormIgnoreAttribute]
        public string Created_UserId { get; set; }

        [MudBlazor.Forms.AeFormIgnoreAttribute]
        public DateTime Created_DT { get; set; }

        [MudBlazor.Forms.AeFormIgnoreAttribute]
        [Timestamp]
        public byte[] Timestamp { get; set; }


        [MudBlazor.Forms.AeFormIgnoreAttribute]
        public bool IsRequirementsComplete => Requirements_Gathering_ActualEndDate != null;
        [MudBlazor.Forms.AeFormIgnoreAttribute]
        public bool IsPhase1DevComplete => Phase1_Development_ActualEndDate != null;
        [MudBlazor.Forms.AeFormIgnoreAttribute]
        public bool IsPhase1TestComplete => Phase1_Testing_ActualEndDate != null;
        [MudBlazor.Forms.AeFormIgnoreAttribute]
        public bool IsPhase2DevComplete => Phase2_Development_ActualEndDate != null;
        [MudBlazor.Forms.AeFormIgnoreAttribute]
        public bool IsPhase2TestComplete => Phase2_Testing_ActualEndDate != null;
        [MudBlazor.Forms.AeFormIgnoreAttribute]
        public bool IsReleased => Actual_Release_Date != null;

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
                    var ret =  estimatedEndDate >= DateTime.Now.Date ? (Color.Success, Severity.Success) : (Color.Error, Severity.Error);
                    var difference = estimatedEndDate - DateTime.Now.Date;
                    if ((ret == (Color.Success, Severity.Success)) && (difference.Value.Days <= 3))
                    { 
                        ret = (Color.Warning, Severity.Warning);  
                    }
                    return ret;
                }
            }
            else
            {
                return estimatedEndDate >= actualEndDate ? (Color.Success, Severity.Success) : (Color.Error, Severity.Error);
            }
        }
    }
}
