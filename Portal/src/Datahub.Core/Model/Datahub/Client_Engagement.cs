using System.ComponentModel.DataAnnotations;
using Datahub.Core.Model.Projects;
using Elemental.Components;
using MudBlazor;
using AeFormCategoryAttribute = MudBlazor.Forms.AeFormCategoryAttribute;

namespace Datahub.Core.Model.Datahub;

public class ClientEngagement
{
    [MudBlazor.Forms.AeFormIgnoreAttribute]
    [Key]
    public int EngagementID { get; set; }

    [AeFormCategory("Projected Engagement Details")]
    [Required]
    [MaxLength(2000)]
    public string EngagementName { get; set; }

    [AeFormCategory("Projected Engagement Details")]
    [Required]
    [MaxLength(2000)]
    public string EngagmentOwners { get; set; }

    [AeFormCategory("Projected Engagement Details")]
    [Required]
    [MaxLength(2000)]
    public string EngagmentLead { get; set; }

    [MudBlazor.Forms.AeFormIgnoreAttribute]
    public DatahubProject Project { get; set; }

    [AeFormCategory("Projected Engagement Details")]
    public bool IsEngagementActive { get; set; }

    [AeFormCategory("Projected Engagement Details")]

    public DateTime? EngagementStartDate { get; set; }
    [AeFormCategory("Projected Engagement Details")]
    public DateTime? RequirementsGatheringEndDate { get; set; }
    [AeFormCategory("Projected Engagement Details")]
    public DateTime? Phase1DevelopmentEndDate { get; set; }
    [AeFormCategory("Projected Engagement Details")]
    public DateTime? Phase1TestingEndDate { get; set; }
    [AeFormCategory("Projected Engagement Details")]
    public DateTime? Phase2DevelopmentEndDate { get; set; }
    [AeFormCategory("Projected Engagement Details")]
    public DateTime? Phase2TestingEndDate { get; set; }
    [AeFormCategory("Projected Engagement Details")]
    public DateTime? FinalUpdatesEndDate { get; set; }
    [AeFormCategory("Projected Engagement Details")]
    public DateTime? FinalReleaseDate { get; set; }
    [AeFormCategory("Actual Engagement Details")]
    public DateTime? RequirementsGatheringActualEndDate { get; set; }
    [AeFormCategory("Actual Engagement Details")]
    public DateTime? Phase1DevelopmentActualEndDate { get; set; }
    [AeFormCategory("Actual Engagement Details")]
    public DateTime? Phase1TestingActualEndDate { get; set; }
    [AeFormCategory("Actual Engagement Details")]
    [AeLabel(placeholder: "Please Select")]
    public DateTime? Phase2DevelopmentActualEndDate { get; set; }
    [AeFormCategory("Actual Engagement Details")]
    public DateTime? Phase2TestingActualEndDate { get; set; }
    [AeFormCategory("Actual Engagement Details")]
    public DateTime? ActualReleaseDate { get; set; }

    [MudBlazor.Forms.AeFormIgnoreAttribute]
    public string LastUpdatedUserId { get; set; }

    [MudBlazor.Forms.AeFormIgnoreAttribute]
    public DateTime LastUpdatedDT { get; set; }

    [MudBlazor.Forms.AeFormIgnoreAttribute]
    public string CreatedUserId { get; set; }

    [MudBlazor.Forms.AeFormIgnoreAttribute]
    public DateTime CreatedDT { get; set; }

    [MudBlazor.Forms.AeFormIgnoreAttribute]
    [Timestamp]
    public byte[] Timestamp { get; set; }

    [MudBlazor.Forms.AeFormIgnoreAttribute]
    public bool IsRequirementsComplete => RequirementsGatheringActualEndDate != null;
    [MudBlazor.Forms.AeFormIgnoreAttribute]
    public bool IsPhase1DevComplete => Phase1DevelopmentActualEndDate != null;
    [MudBlazor.Forms.AeFormIgnoreAttribute]
    public bool IsPhase1TestComplete => Phase1TestingActualEndDate != null;
    [MudBlazor.Forms.AeFormIgnoreAttribute]
    public bool IsPhase2DevComplete => Phase2DevelopmentActualEndDate != null;
    [MudBlazor.Forms.AeFormIgnoreAttribute]
    public bool IsPhase2TestComplete => Phase2TestingActualEndDate != null;
    [MudBlazor.Forms.AeFormIgnoreAttribute]
    public bool IsReleased => ActualReleaseDate != null;

    public (Color Color, Severity Severity) TimelineStatus(bool isComplete, DateTime? estimatedEndDate, DateTime? actualEndDate)
    {
        if (!isComplete)
        {
            if (estimatedEndDate is null)
            {
                return (Color.Info, Severity.Info);
            }
            else
            {
                var ret = estimatedEndDate >= DateTime.Now.Date ? (Color.Success, Severity.Success) : (Color.Error, Severity.Error);
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