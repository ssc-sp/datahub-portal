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
        [MaxLength(35)]
        [AeFormCategory("M365 Teams Information", 10)]
        [AeLabel(placeholder: "[Enter a short bilingual name for the team. For example: IM Working Group | Groupe de travail sur la GI]")]
        public string Name_of_Team { get; set; }

        [Required]
        [AeFormCategory("M365 Teams Information", 10)]
        [AeLabel(placeholder: "[Enter a short bilingual description to display in the MS Teams 'About' section. For example: Collaboration space for sector financial advisors | Espace de collaboration pour les conseillers financier des secteurs]")]
        public string Description_of_Team { get; set; }

        [AeFormCategory("M365 Teams Information", 10)]
        [AeLabel(isDropDown: true, placeholder: "[How do you plan to use the team? Select all that apply:]", validValues: new[] { "Working Group", "Committee", "Event", "Project or Initiative", "Other (please specify)" })]
        public string Team_Purpose { get; set; }

        [AeFormCategory("M365 Teams Information", 10)]
        [AeLabel(isDropDown: true, placeholder: "[Select the appropriate Business Activity:]", validValues: new[] { "Acquisition and Procurement", "Communication", "Emergency Management", "Financial Management", "Human Resources", "Information Management", "Information Technology", "Knowledge Dissemination", "Legal", "Management and Oversight", "Material Management", "Policy", "Program Administration", "Real Property Management", "Regulatory", "Science and Technology", "Stakeholder Relations", "Travel and Administrative Services" })]
        public string Team_Function { get; set; }

        [AeFormCategory("Membership", 20)]
        [AeLabel(isDropDown: true, placeholder: "[Select a range for the number of members in the team:]", validValues: new[] { "1-15", "16-30", "31-50", "51-100", "100+" })]
        public string Number { get; set; }

        [AeFormCategory("Membership", 20)]
        [AeLabel(placeholder: "[Enter what the membership composition will consist of. For example: Committee Members; M365 Power Users; All NRCan Management; Project Team and Sector stakeholders ]")]
        public string Composition { get; set; }

        [Required]
        [AeFormCategory("Security", 30)]
        [AeLabel(placeholder: "[Select an appropriate data security sensitivity applicable to ALL files, meetings and conversations within this team: Security and Information Classification Guide – a handy reference tool to assist you in categorizing and safeguarding information. ]", validValues: new[] { "Protected A", "Protected B", "Unclassified" })]
        public string Information_and_Data_Security_Classification { get; set; }

        [Required]
        [AeFormCategory("Security ", 40)]
        [AeLabel(placeholder: "[Select whether this team will be accessible to:]", validValues: new[] { "Private (select members only)", "Public (all NRCan staff)" })]
        public bool Visibility { get; set; }

        [AeFormCategory("GCdocs Folder Location", 50)]
        [AeLabel(placeholder: "[Insert the GCdocs hyperlink (i.e. https://gcdocs.gc.ca/nrcan-rncan/llisapi.dll/Overview/XXXXXXXX) of where business value information will be saved.]")]
        public string GCdocs_Hyperlink_URL { get; set; }

        [AeFormCategory("Lifespan of Team", 60)]
        [AeLabel(placeholder: "[Select the expected retirement date for the team:]")]
        public DateTime Expected_Lifespan_DT { get; set; }

        [AeFormCategory("Lifespan of Team", 60)]
        [AeLabel(isDropDown: true, placeholder: "[Or Select Ongoing ]")]
        public string Expected_Lifespan { get; set; }


        [AeFormCategory("Owners", 70)]
        [AeLabel(placeholder: "[Enter the name (Firsname Lastname) of the of DG-level business owner responsible for the Team and all the information within it. ]")]
        public string Business_Owner { get; set; }

        [Required]
        [AeFormCategory("Owners", 70)]        
        public string Team_Owner1 { get; set; }
        [Required]
        [AeFormCategory("Owners", 70)]
        public string Team_Owner2 { get; set; }
        
        [AeFormCategory("Owners", 70)]
        public string Team_Owner3 { get; set; }


        [AeFormCategory("Approval", 80)]
        [AeLabel(placeholder: "[I, the business owner identified above, approve this M365 Teams request.]")]
        public string Business_Owner_Approval { get; set; }
        
        [AeFormIgnore]
        public string Last_Updated_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
