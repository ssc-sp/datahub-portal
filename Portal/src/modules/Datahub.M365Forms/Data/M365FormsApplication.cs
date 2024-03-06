using System.ComponentModel.DataAnnotations;
using Elemental.Components;

namespace Datahub.M365Forms.Data;

public class M365FormsApplication
{
    [Key]
    [AeFormIgnore]
    public int ApplicationID { get; set; }

    [Required]
    [MaxLength(256)]
    [AeFormCategory("M365 Teams Information", 10)]
    [AeLabel(placeholder: "Enter a short bilingual name for the team (max 256 characters, recommended 35 characters). For example: IM Working Group | Groupe de travail sur la GI")]
    public string NameOfTeam { get; set; } = null!;

    [Required]
    [MaxLength(1000)]
    [AeFormCategory("M365 Teams Information", 10)]
    [AeLabel(placeholder: "Enter a short bilingual description to display in the MS Teams 'About' section (max 1000 characters). For example: Collaboration space for sector financial advisors | Espace de collaboration pour les conseillers financier des secteurs")]
    public string DescriptionOfTeam { get; set; } = null!;

    [AeFormCategory("M365 Teams Information", 10)]
    [AeLabel(isDropDown: true, placeholder: "Select the appropriate Business Activity:", validValues: new[] { "Acquisition and Procurement", "Communications", "Emergency Management", "Financial Management", "Human Resources", "Information Management", "Information Technology", "Knowledge Dissemination", "Legal", "Management and Oversight", "Material Management", "Policy", "Program Administration", "Real Property Management", "Regulatory", "Science and Technology", "Stakeholder Relations", "Travel and Administrative Services" })]
    public string? TeamFunction { get; set; }

    [AeFormCategory("Team Purpose - How do you plan to use the team? Select all that apply", 15)]
    public bool WorkingGroup { get; set; }

    [AeFormCategory("Team Purpose - How do you plan to use the team? Select all that apply", 15)]
    public bool Committee { get; set; }

    [AeFormCategory("Team Purpose - How do you plan to use the team? Select all that apply", 15)]
    public bool Event { get; set; }

    [AeFormCategory("Team Purpose - How do you plan to use the team? Select all that apply", 15)]
    public bool ProjectOrInitiative { get; set; }

    [AeFormCategory("Team Purpose - How do you plan to use the team? Select all that apply", 15)]
    public bool Other { get; set; }

    [AeFormCategory("Team Purpose - How do you plan to use the team? Select all that apply", 15)]
    [AeLabel(placeholder: "Please specify other purpose")]
    public string? OtherTxt { get; set; }

    [AeFormCategory("Membership", 20)]
    [AeLabel(isDropDown: true, placeholder: "Select a range for the number of members in the team:", validValues: new[] { "1-15", "16-30", "31-50", "51-100", "100+" })]
    public string? Number { get; set; }

    [AeFormCategory("Membership", 20)]
    [AeLabel(placeholder: "Enter what the membership composition will consist of. For example: Committee Members; M365 Power Users; All NRCan Management; Project Team and Sector stakeholders")]
    public string? Composition { get; set; }

    [Required]
    [AeFormCategory("Security", 30)]
    [AeLabel(placeholder: "Select an appropriate data security sensitivity applicable to ALL files, meetings and conversations within this team: Security and Information Classification Guide – a handy reference tool to assist you in categorizing and safeguarding information.", validValues: new[] { "Unclassified", "Protected A", "Protected B" })]
    public string InformationAndDataSecurityClassification { get; set; } = null!;

    [Required]
    [AeFormCategory("Security", 30)]
    [AeLabel(isDropDown: true, placeholder: "Select whether this team will be accessible to:", validValues: new[] { "Private (select members only)", "Public (all NRCan staff)" })]
    public string Visibility { get; set; } = null!;

    [Required]
    [AeFormCategory("GCdocs Folder Location", 50)]
    [AeLabel(placeholder: "Insert the GCdocs hyperlink (i.e. https://gcdocs.gc.ca/nrcan-rncan/llisapi.dll/Overview/XXXXXXXX) of where business value information will be saved.")]
    public string GCdocsHyperlinkURL { get; set; } = null!;

    [AeFormCategory("Lifespan of Team", 60)]
    public bool OngoingLifespan { get; set; }

    [AeFormCategory("Lifespan of Team", 60)]
    [AeLabel(placeholder: "Select the expected retirement date for the team:")]
    public DateTime? ExpectedLifespanDT { get; set; }

    [AeFormCategory("Owners", 70)]
    [Required]
    [AeLabel(isDropDown: true, placeholder: "[Enter your Sector acronym and/or name]")]
    [StringLength(2000)]
    public string ClientSector { get; set; } = null!;

    [AeFormCategory("Owners", 70)]
    [Required]
    [AeLabel(placeholder: "Enter the name (Firsname Lastname) of the of DG-level business owner responsible for the Team and all the information within it.")]
    public string BusinessOwner { get; set; } = null!;

    [Required]
    [AeLabel(placeholder: "Enter required team owner.")]
    [AeFormCategory("Owners", 70)]
    public string TeamOwner1 { get; set; } = null!;

    [Required]
    [AeLabel(placeholder: "Enter required team owner.")]
    [AeFormCategory("Owners", 70)]
    public string TeamOwner2 { get; set; } = null!;

    [AeFormCategory("Owners", 70)]
    [AeLabel(placeholder: "Enter optional team owner.")]
    public string? TeamOwner3 { get; set; }

    [AeFormCategory("Approval", 80)]
    public bool BusinessOwnerApproval { get; set; }

    [AeFormCategory("Application Status", 90)]
    [AeLabel(isDropDown: true, placeholder: "Select status of application:", validValues: new[] { "Team Requested", "Submitted to Assyst", "Request Pending", "Team Created", "Request Denied", "Team Deleted" })]
    public string? M365FormStatus { get; set; }

    [AeFormCategory("Application Status", 90)]
    public string? Comments { get; set; }

    [AeFormCategory("Application Status", 90)]
    public bool IsOrganizationalTeam { get; set; }

    [AeFormCategory("Application Status", 90)]
    public DateTime SubmittedDT { get; set; }

    [AeFormCategory("Application Status", 90)]
    public DateTime LastUpdatedDT { get; set; }


    [AeFormIgnore]
    public string? SubmittedBy { get; set; }

    [AeFormIgnore]
    public bool NotificationsSent { get; set; }

    [AeFormIgnore]
    public string LastUpdatedUserId { get; set; } = null!;



    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; } = null!;

}