using Elemental.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NRCan.Datahub.Portal.Data.LanguageTraining
{
    public class LanguageTrainingApplication
    {
        [Key]
        [AeFormIgnore]
        public int Application_ID { get; set; }


        [AeFormCategory("Employee Information", 10)]
        [Required]
        [MaxLength(200)]
        public string NRCan_Username { get; set; }
        [AeFormCategory("Employee Information", 10)]
        [Required]
        public string First_Name { get; set; }
        [AeFormCategory("Employee Information", 10)]
        [Required]
        public string Last_Name { get; set; }
        [AeFormCategory("Employee Information", 10)]
        [Required]
        public string Email_Address_EMAIL { get; set; }
        [AeFormCategory("Employee Information", 10)]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        [Required]
        public string Sector_Branch { get; set; }
        [AeFormCategory("Employee Information", 10)]
        [Required]
        public string City { get; set; }
        [AeFormCategory("Employee Information", 10)]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        [Required]
        public string Province_Territory { get; set; }
        [AeFormCategory("Employee Information", 10)]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        [Required]
        public string Employment_Status { get; set; }
        [AeFormCategory("Employee Information", 10)]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        [Required]
        public string I_am_seeking { get; set; }
        [AeFormCategory("Language Assessment", 20)]                
        public bool Completed_LETP_Assessment { get; set; }
        [AeFormCategory("Language Assessment", 20)]        
        public bool Language_Training_Since_LETP_Assessment { get; set; }
        [AeFormCategory("Language Assessment", 20)]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]        
        public string Language_Training_Provided_By { get; set; }

        [AeFormCategory("Language Assessment", 20)]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]        
        public string Last_Course_Successfully_Completed { get; set; }
        
        [AeFormCategory("Language Assessment", 20)]        
        public int Completed_Training_Year { get; set; }
        
        [AeFormCategory("Language Assessment", 20)]
        public string Completed_Training_Session { get; set; }
        
        [AeFormCategory("Language Assessment", 20)]        
        public bool Report_Sent_To_NRCan_Language_School { get; set; }
        
        [AeFormCategory("Language Assessment", 20)]
        public bool Employee_Appointed_NonImperative_Basis { get; set; }
        [AeFormCategory("Language Assessment", 20)]
        public bool Employee_language_profile_raised { get; set; }
        [AeFormCategory("Language Assessment", 20)]
        public bool Employee_professional_dev_program { get; set; }
        [AeFormCategory("Language Assessment", 20)]
        public bool Employee_talent_management_exercise { get; set; }
        [AeFormCategory("Language Assessment", 20)]
        public bool Employee_equity_group { get; set; }



        [AeFormCategory("Language Assessment", 20)]
        
        public bool Second_Language_Evaluation_Results { get; set; }
        [AeFormCategory("Language Assessment", 20)]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        
        public string SLE_Results_Reading { get; set; }
        [AeFormCategory("Language Assessment", 20)]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        
        public string SLE_Results_Writing { get; set; }
        [AeFormCategory("Language Assessment", 20)]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        
        public string SLE_Results_Oral { get; set; }
        
        [AeFormCategory("Language Training Application", 30)]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        [Required]
        public string Training_Type { get; set; }
        [AeFormCategory("Language Training Application", 30)]
        public DateTime SLE_Test_Date { get; set; }
        [AeFormCategory("Language Training Application", 30)]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        public string Session_For_Language_Training { get; set; }
        [AeFormCategory("Language Training Application", 30)]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        public string Class_For_Language_Training { get; set; }

        [AeFormCategory("Commitment and Approval", 40)]
        [Required]
        public string Delegate_Manager_First_Name { get; set; }
        [AeFormCategory("Commitment and Approval", 40)]
        [Required]
        public string Delegated_Manager_Last_Name { get; set; }
        [AeFormCategory("Commitment and Approval", 40)]
        [Required]
        public string Delegated_Manager_Email { get; set; }




        [AeFormCategory("Manager Section", 50)]
        public string Manager_First_Name { get; set; }

        [AeFormCategory("Manager Section", 50)]
        public string Manager_Last_Name { get; set; }

        [AeFormCategory("Manager Section", 50)]
        public string Manager_Email_Address { get; set; }


        /** Section: Language School Selection **/
        public string Decision { get; set; }



        [AeFormIgnore]
        public string Last_Updated_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
