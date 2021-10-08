using Elemental.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Shared.Model.Onboarding
{
    public class OnboardingApp
    {
        [Key]
        [AeFormIgnore]
        public int Application_ID { get; set; }
        [AeFormCategory("Client Information", 10)]
        [Required]
        [AeLabel(isDropDown: true, placeholder: "[Enter your Sector acronym and/or name]")]
        [StringLength(2000)]
        public string Client_Sector { get; set; }
        [StringLength(2000)]
        [AeFormCategory("Client Information", 10)]
        [Required]
        [AeLabel(isDropDown: true, placeholder: "[Enter your Branch acronym and/or name]")]
        public string Client_Branch { get; set; }
        [StringLength(2000)]
        [AeFormCategory("Client Information", 10)]
        [Required]
        [AeLabel(placeholder: "[Enter your Division acronym and/or name]")]
        public string Client_Division { get; set; }
        [StringLength(200)]
        [AeFormCategory("Client Information", 10)]
        [Required]
        [AeLabel(placeholder: "[Lastname, Firstname]")]
        public string Client_Contact_Name { get; set; }
        [StringLength(200)]
        [AeFormCategory("Client Information", 10)]
        [Required]
        [AeLabel(placeholder: "[Firstname.Lastname@nrcan-rncan.gc.ca]")]
        public string Client_Email { get; set; }
        [StringLength(200)]
        [AeFormCategory("Client Information", 10)]
        [AeLabel(placeholder: "[Lastname, Firstname]")]
        public string Additional_Contact_Name { get; set; }
        [StringLength(200)]
        [AeFormCategory("Client Information", 10)]
        [AeLabel(placeholder: "[Firstname.Lastname@nrcan-rncan.gc.ca]")]
        public string Additional_Contact_Email_EMAIL { get; set; }


        [AeFormCategory("Project Information", 20)]
        [AeLabel(placeholder: "[Enter the name of your project]")]
        public string Project_Name { get; set; }


        [AeFormCategory("Project Information", 20)]
        [AeLabel(placeholder: "[Provide a brief summary/description of your project]")]
        public string Project_Summary_Description { get; set; }

        [AeFormCategory("Project Information", 20)]
        [AeLabel(placeholder: "[Provide a brief description of the objective(s) you would like to accomplish with DataHub]")]
        public string Project_Goal { get; set; }

        [AeFormCategory("Project Information", 20)]
        [AeLabel(placeholder: "[Provide any anticipated timelines or deadlines for onboarding to DataHub]")]
        public string Onboarding_Timeline { get; set; }
        

        [AeFormCategory("Project Information", 20)]
        public bool Project_Engagement_Category_DataPipeline { get; set; }
        [AeFormCategory("Project Information", 20)]
        public bool Project_Engagement_Category_DataScience { get; set; }
        [AeFormCategory("Project Information", 20)]
        public bool Project_Engagement_Category_FullStack { get; set; }
        [AeFormCategory("Project Information", 20)]
        public bool Project_Engagement_Category_Guidance { get; set; }
        [AeFormCategory("Project Information", 20)]
        public bool Project_Engagement_Category_PowerBIReports { get; set; }
        [AeFormCategory("Project Information", 20)]
        public bool Project_Engagement_Category_Storage { get; set; }
        [AeFormCategory("Project Information", 20)]
        public bool Project_Engagement_Category_WebForms { get; set; }
        [AeFormCategory("Project Information", 20)]
        public bool Project_Engagement_Category_Unknown { get; set; }
        [AeFormCategory("Project Information", 20)]
        public bool Project_Engagement_Category_Other { get; set; }

        [AeFormCategory("Project Information", 20)]
        [StringLength(200)]
        public string Project_Engagement_Category_OtherText { get; set; }

        [AeFormCategory("Project Information", 20)]
        public bool Data_Set_Security_Level_Classified { get; set; }
        [AeFormCategory("Project Information", 20)]
        public bool Data_Set_Security_Level_ProtectedA { get; set; }
        [AeFormCategory("Project Information", 20)]
        public bool Data_Set_Security_Level_ProtectedB { get; set; }
        [AeFormCategory("Project Information", 20)]
        public bool Data_Set_Security_Level_ProtectedC { get; set; }
        [AeFormCategory("Project Information", 20)]
        public bool Data_Set_Security_Level_UnClassified { get; set; }



        [AeFormCategory("Additional Information", 30)]
        [AeLabel(placeholder: "[Enter any additional information/comments regarding your project or questions for the DataHub team]")]
        public string Questions_for_the_DataHub_Team { get; set; }

        [AeLabel(placeholder: "[Insert any attachments and/or links to any folders or documentation or sample data or artifacts related to your project]")]
        [AeFormCategory("Additional Information", 30)]
        public string Attachments { get; set; }

        [AeFormIgnore]
        public string Last_Updated_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }

    }
}
