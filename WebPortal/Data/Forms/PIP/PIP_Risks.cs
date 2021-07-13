using Elemental.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NRCan.Datahub.ProjectForms.Data.PIP
{
    public class PIP_Risks
    {
        [Key]
        [AeFormIgnore]
        public int Risks_ID { get; set; }


        [AeLabel(row: "1", column: "1")]
        [Required] [MaxLength(200)] public string Risk_Title { get; set; }

        [AeLabel(placeholder: "Please provide a description of the risk including the impact on the department, gov, stakeholders etc. as well as the type of risk this is ie strategic/ operational", row: "2", column: "1")]
        [Required] [MaxLength(7500)] public string Risk_Description_TXT { get; set; }
        
        [AeLabel(placeholder: "Please identify here the risk area (ie. HR, complexity, climate change, program delivery, funding, administration etc.)", row: "3", column: "1")]
        [Required] [MaxLength(50)] public string Risk_Id_TXT { get; set; }
        
        [Required]
        [MaxLength(200)]
        [AeLabel(isDropDown: true, placeholder:"Please Select", row: "4", column: "1")]
        public string Risk_Category { get; set; }

        
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "4", column: "2")]
        [MaxLength(7500)] public string Risk_Trend_TXT { get; set; }

        [AeFormCategory("Risk Drivers", 5)]
        [AeLabel(placeholder: "Please write only one risk driver per field. If you have more than one driver to report, please use the optional risk driver fields.", row: "5", column: "1")]
        [Required] [MaxLength(7500)] public string Risk_Drivers_TXT { get; set; }

        [AeFormCategory("Risk Drivers", 5)]
        [AeLabel(row: "6", column: "1")]
        [MaxLength(7500)] public string Risk_Drivers_TXT2 { get; set; }

        [AeFormCategory("Risk Drivers", 5)]
        [AeLabel(row: "7", column: "1")]
        [MaxLength(7500)] public string Risk_Drivers_TXT3 { get; set; }
        [AeFormCategory("Risk Drivers", 5)]
        [AeLabel(row: "8", column: "1")]
        [MaxLength(7500)] public string Risk_Drivers_TXT4 { get; set; }

        //[MaxLength(7500)] public string Residual_Risk_Level_TXT { get; set; }
        //[AeLabel(placeholder: "What is the level of risk that would be optimal, or that the department/ program should aim for")]

        [AeFormCategory("Risk Level", 10)]
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "9", column: "1")]
        [Required] public string Impact1 { get; set; }
        [AeFormCategory("Risk Level", 10)]
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "9", column: "2")]
        [Required] public string Likelihood1 { get; set; }
        [AeFormCategory("Risk Level", 10)]
        [AeLabel(row: "9", column: "3")]
        [Editable(false)]
        public int InherentLevel1 => GetInherentLevel(Impact1, Likelihood1);

        

        [AeFormCategory("Risk Level", 10)]
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "10", column: "1")]
        [Required] public string Impact2 { get; set; }
        [AeFormCategory("Risk Level", 10)]
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "10", column: "2")]
        [Required] public string Likelihood2 { get; set; }
        [AeFormCategory("Risk Level", 10)]
        [Editable(false)]
        [AeLabel(row: "10", column: "3")]
        public int InherentLevel2 => GetInherentLevel(Impact2, Likelihood2);

        [AeFormCategory("Risk Level", 10)]
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "11", column: "1")]
        [Required] public string Impact3 { get; set; }
        [AeFormCategory("Risk Level", 10)]
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "11", column: "2")]
        [Required] public string Likelihood3 { get; set; }
        [AeFormCategory("Risk Level", 10)]
        [Editable(false)]
        [AeLabel(row: "11", column: "3")]
        public int InherentLevel3 => GetInherentLevel(Impact3, Likelihood3);




        [AeFormCategory("Ongoing Monitoring Activities", 15)]
        [AeLabel(isDropDown:true, placeholder: "Please select", row: "12", column: "1")]
        [Required] [MaxLength(7500)] public string Ongoing_Monitoring_Activities_TXT { get; set; }

        [AeFormCategory("Ongoing Monitoring Activities", 15)]
        [AeLabel(isDropDown: true, placeholder: "Please select", row: "12", column: "2")]
        [Required] [MaxLength(7500)] public string Ongoing_Monitoring_Timeframe_TXT { get; set; }

        [AeFormCategory("Ongoing Monitoring Activities", 15)]
        [AeLabel(isDropDown: true, placeholder: "Please select", row: "13", column: "1")]
        [MaxLength(7500)] public string Ongoing_Monitoring_Activities_TXT2 { get; set; }

        [AeFormCategory("Ongoing Monitoring Activities", 15)]
        [AeLabel(isDropDown: true, placeholder: "Please select", row: "13", column: "2")]
        [MaxLength(7500)] public string Ongoing_Monitoring_Timeframe2_TXT { get; set; }

        [AeFormCategory("Ongoing Monitoring Activities", 15)]
        [AeLabel(isDropDown: true, placeholder: "Please select", row: "14", column: "1")]
        [MaxLength(7500)] public string Ongoing_Monitoring_Activities_TXT3 { get; set; }

        [AeFormCategory("Ongoing Monitoring Activities", 15)]
        [AeLabel(isDropDown: true, placeholder: "Please select", row: "14", column: "2")]
        [MaxLength(7500)] public string Ongoing_Monitoring_Timeframe3_TXT { get; set; }




        [AeFormCategory("Mitigation Strategies", 20)]
        [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk", row: "15", column: "1")]
        [Required] [MaxLength(7500)] 
        public string Future_Mitigation_Activities_TXT { get; set; }
        
        [AeFormCategory("Mitigation Strategies", 20)]
        [Required][AeLabel(isDropDown: true, placeholder: "Please Select", row: "15", column: "2")]
        public string Strategy_Timeline1 { get; set; }


        [AeFormCategory("Mitigation Strategies", 20)]
        [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk", row: "16", column: "1")]
        [MaxLength(7500)] public string Future_Mitigation_Activities_TXT2 { get; set; }
        [AeFormCategory("Mitigation Strategies", 20)]
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "16", column: "2")]
        public string Strategy_Timeline2 { get; set; }


        [AeFormCategory("Mitigation Strategies", 20)]
        [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk", row: "17", column: "1")]
        [MaxLength(7500)] public string Future_Mitigation_Activities_TXT3 { get; set; }
        [AeFormCategory("Mitigation Strategies", 20)]
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "17", column: "2")]
        public string Strategy_Timeline3 { get; set; }


        [AeFormCategory("Mitigation Strategies", 20)]
        [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk", row: "18", column: "1")]
        [MaxLength(7500)] public string Future_Mitigation_Activities_TXT4 { get; set; }
        [AeFormCategory("Mitigation Strategies", 20)]
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "18", column: "2")]
        public string Strategy_Timeline4 { get; set; }

        [AeFormCategory("Mitigation Strategies", 20)]
        [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk", row: "19", column: "1")]
        [MaxLength(7500)] public string Future_Mitigation_Activities_TXT5 { get; set; }
        [AeFormCategory("Mitigation Strategies", 20)]
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "19", column: "2")]
        public string Strategy_Timeline5 { get; set; }



        [AeFormCategory("Corporate Information", 25)]
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "20", column: "1")]
        [MaxLength(7500)] public string Relevant_Corporate_Priorities_TXT {get;set;}
        
        [AeFormCategory("Corporate Information", 25)]
        [AeLabel(isDropDown: true, placeholder: "Please Select", row: "21", column: "1")]
        [MaxLength(7500)] public string Relevant_Corporate_Risks_TXT {get;set;}

        [AeFormCategory("", 30)]
        [AeLabel(placeholder: "Please provide comments here as necessray to supplement any of the information requested", row: "22", column: "1")]        
        [MaxLength(7500)] public string Comments_TXT { get; set; }
        

        [AeLabel(row: "23", column: "1")]
        [AeFormCategory("Latest Update Information", 60)]
        [Editable(false)] public string Last_Updated_UserId { get; set; }
        [AeLabel(row: "23", column: "2")]
        [AeFormCategory("Latest Update Information", 60)]
        [Required] public DateTime Date_Updated_DT { get; set; }


        public PIP_Tombstone PIP_Tombstone { get; set; }
        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }

        public List<PIP_IndicatorRisks> PIP_IndicatorRisks { get; set; }

        private int GetInherentLevel(string impact, string likelihood)
        {
            int intImpact;
            int intLikelihood;
            bool impactSuccess = Int32.TryParse(impact, out intImpact);
            bool likelihoodSuccess = Int32.TryParse(likelihood, out intLikelihood);

            if (impactSuccess && likelihoodSuccess)
                return intImpact * intLikelihood;

            return 0;
        }
    }
}
