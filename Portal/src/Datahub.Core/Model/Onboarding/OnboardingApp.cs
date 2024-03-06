using System.ComponentModel.DataAnnotations;
using Elemental.Components;

namespace Datahub.Core.Model.Onboarding;

public class OnboardingApp
{
    [Key]
    [AeFormIgnore]
    public int ApplicationID { get; set; }
    [AeFormCategory("Client Information", 10)]
    [Required]
    [AeLabel(isDropDown: true, placeholder: "[Enter your Sector acronym and/or name]")]
    [StringLength(2000)]
    public string ClientSector { get; set; }
    [StringLength(2000)]
    [AeFormCategory("Client Information", 10)]
    [AeLabel(isDropDown: true, placeholder: "[Select your Branch]")]
    public string ClientBranch { get; set; }
    [StringLength(2000)]
    [AeFormCategory("Client Information", 10)]
    [AeLabel(isDropDown: true, placeholder: "[Select your Division]")]
    public string ClientDivision { get; set; }
    [StringLength(200)]
    [AeFormCategory("Client Information", 10)]
    [Required]
    [AeLabel(placeholder: "[Lastname, Firstname]")]
    public string ClientContactName { get; set; }
    [StringLength(200)]
    [AeFormCategory("Client Information", 10)]
    [Required]
    [AeLabel(placeholder: "[Firstname.Lastname@nrcan-rncan.gc.ca]")]
    public string ClientEmail { get; set; }
    [StringLength(200)]
    [AeFormCategory("Client Information", 10)]
    [AeLabel(placeholder: "[Lastname, Firstname]")]
    public string AdditionalContactName { get; set; }
    [StringLength(200)]
    [AeFormCategory("Client Information", 10)]
    [AeLabel(placeholder: "[Firstname.Lastname@nrcan-rncan.gc.ca]")]
    public string AdditionalContactEmailEMAIL { get; set; }

    [AeFormCategory("Product Information", 20)]
    [AeLabel(placeholder: "[Enter the name of your product]")]
    public string ProductName { get; set; }

    [AeFormCategory("Product Information", 20)]
    [AeLabel(placeholder: "[Provide a brief summary/description of your product]")]
    public string ProjectSummaryDescription { get; set; }

    [AeFormCategory("Product Information", 20)]
    [AeLabel(placeholder: "[Provide a brief description of the objective(s) you would like to accomplish with DataHub]")]
    public string ProjectGoal { get; set; }

    [AeFormCategory("Product Information", 20)]
    [AeLabel(placeholder: "[Provide any anticipated timelines or deadlines for onboarding to DataHub]")]
    public string OnboardingTimeline { get; set; }

    [AeFormCategory("Product Information", 20)]
    [AeLabel(isDropDown: true)]
    public string ProjectEngagementCategory { get; set; }

    [AeFormCategory("Product Information", 20)]
    public string ProjectEngagementCategoryOther { get; set; }

    [AeFormCategory("Product Information", 20)]
    [AeLabel(isDropDown: true)]
    public string DataSecurityLevel { get; set; }

    [AeFormCategory("Additional Information", 30)]
    [AeLabel(placeholder: "[Enter any additional information/comments regarding your product or questions for the DataHub team]")]
    public string QuestionsForTheDataHubTeam { get; set; }

    [AeFormIgnore]
    public bool NotificationsSent { get; set; }

    [AeFormIgnore]
    public string LastUpdatedUserId { get; set; }

    [AeFormIgnore]
    public DateTime LastUpdatedDT { get; set; }

    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }

    [AeFormIgnore]
    public DateTime? ProjectCreatedDate { get; set; }
}