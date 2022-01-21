using Elemental.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Portal.Data
{
    public class M365FormsApplication
    {
        [Key]
        [AeFormIgnore]
        public int Application_ID { get; set; }
        
        [Required]
        [MaxLength(256)]
        [AeFormCategory("M365 Teams Information", 10)]
        [AeLabel(placeholder: "Enter a short bilingual name for the team (max 256 characters, recommended 35 characters). For example: IM Working Group | Groupe de travail sur la GI")]
        public string Name_of_Team { get; set; }

        [Required]
        [MaxLength(1000)]
        [AeFormCategory("M365 Teams Information", 10)]
        [AeLabel(placeholder: "Enter a short bilingual description to display in the MS Teams 'About' section (max 1000 characters). For example: Collaboration space for sector financial advisors | Espace de collaboration pour les conseillers financier des secteurs")]
        public string Description_of_Team { get; set; }        

        [AeFormCategory("M365 Teams Information", 10)]
        [AeLabel(isDropDown: true, placeholder: "Select the appropriate Business Activity:", validValues: new[] { "Acquisition and Procurement", "Communications", "Emergency Management", "Financial Management", "Human Resources", "Information Management", "Information Technology", "Knowledge Dissemination", "Legal", "Management and Oversight", "Material Management", "Policy", "Program Administration", "Real Property Management", "Regulatory", "Science and Technology", "Stakeholder Relations", "Travel and Administrative Services" })]
        public string? Team_Function { get; set; }

        [AeFormCategory("Team Purpose - How do you plan to use the team? Select all that apply", 15)]        
        public bool Working_Group { get; set; }
        [AeFormCategory("Team Purpose - How do you plan to use the team? Select all that apply", 15)]
        public bool Committee { get; set; }
        [AeFormCategory("Team Purpose - How do you plan to use the team? Select all that apply", 15)]
        public bool Event { get; set; }
        [AeFormCategory("Team Purpose - How do you plan to use the team? Select all that apply", 15)]
        public bool Project_Or_Initiative { get; set; }
        [AeFormCategory("Team Purpose - How do you plan to use the team? Select all that apply", 15)]
        public bool Other { get; set; }
        [AeFormCategory("Team Purpose - How do you plan to use the team? Select all that apply", 15)]
        [AeLabel(placeholder: "Please specify other purpose")]
        public string? Other_Txt { get; set; }


        [AeFormCategory("Membership", 20)]
        [AeLabel(isDropDown: true, placeholder: "Select a range for the number of members in the team:", validValues: new[] { "1-15", "16-30", "31-50", "51-100", "100+" })]
        public string? Number { get; set; }

        [AeFormCategory("Membership", 20)]
        [AeLabel(placeholder: "Enter what the membership composition will consist of. For example: Committee Members; M365 Power Users; All NRCan Management; Project Team and Sector stakeholders")]
        public string? Composition { get; set; }

        [Required]
        [AeFormCategory("Security", 30)]
        [AeLabel(placeholder: "Select an appropriate data security sensitivity applicable to ALL files, meetings and conversations within this team: Security and Information Classification Guide – a handy reference tool to assist you in categorizing and safeguarding information.", validValues: new[] { "Protected A", "Protected B", "Unclassified" })]
        public string? Information_and_Data_Security_Classification { get; set; }

        [Required]
        [AeFormCategory("Security", 30)]
        [AeLabel(isDropDown: true, placeholder: "Select whether this team will be accessible to:", validValues: new[] { "Private (select members only)", "Public (all NRCan staff)" })]
        public string? Visibility { get; set; }

        [AeFormCategory("GCdocs Folder Location", 50)]
        [AeLabel(placeholder: "Insert the GCdocs hyperlink (i.e. https://gcdocs.gc.ca/nrcan-rncan/llisapi.dll/Overview/XXXXXXXX) of where business value information will be saved.")]
        public string? GCdocs_Hyperlink_URL { get; set; }

        [AeFormCategory("Lifespan of Team", 60)]
        public bool Ongoing_Lifespan { get; set; }

        [AeFormCategory("Lifespan of Team", 60)]
        [AeLabel(placeholder: "Select the expected retirement date for the team:")]
        public DateTime Expected_Lifespan_DT { get; set; }


        [AeFormCategory("Owners", 70)]
        [Required]
        [AeLabel(placeholder: "Enter the name (Firsname Lastname) of the of DG-level business owner responsible for the Team and all the information within it.")]
        public string Business_Owner { get; set; }

        [Required]
        [AeLabel(placeholder: "Enter required team owner.")]
        [AeFormCategory("Owners", 70)]        
        public string Team_Owner1 { get; set; }
        [Required]
        [AeLabel(placeholder: "Enter required team owner.")]
        [AeFormCategory("Owners", 70)]
        public string Team_Owner2 { get; set; }
        
        [AeFormCategory("Owners", 70)]
        [AeLabel(placeholder: "Enter optional team owner.")]
        public string? Team_Owner3 { get; set; }


        [AeFormCategory("Approval", 80)]        
        public bool Business_Owner_Approval { get; set; }
        
        [AeFormCategory("Application Status", 90)]
        [AeLabel(isDropDown: true, placeholder: "Select status of application:", validValues: new[] { "Team Requested", "Submitted to Assyst", "Team Created" })]
        public string? M365FormStatus { get; set; }

        [AeFormIgnore]
        public string? SubmittedBy { get; set; }

        [AeFormIgnore]
        public bool NotificationsSent { get; set; }

        [AeFormIgnore]
        public string Last_Updated_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }

    }
}
