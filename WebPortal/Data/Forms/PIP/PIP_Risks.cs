using Elemental.Components.Forms;
using System;
using System.Collections.Generic;
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



        [Required] [MaxLength(200)] public string? Risk_Title { get; set; }

        [AeLabel(placeholder: "Please provide a description of the risk including the impact on the department, gov, stakeholders etc. as well as the type of risk this is ie strategic/ operational")]
        [Required] [MaxLength(7500)] public string? Risk_Description_TXT { get; set; }
        
        [AeLabel(placeholder: "Please identify here the risk area (ie. HR, complexity, climate change, program delivery, funding, administration etc.)")]
        [Required] [MaxLength(50)] public string? Risk_Id_TXT { get; set; }

        [AeLabel(isDropDown: true, placeholder:"Please Select")][Required] [MaxLength(200)] public string? Risk_Category { get; set; }

        [AeLabel(placeholder: "Please identify the drivers of this risk (what are the conditions of the environment that will increase the likelihood of the risk coming to pass")]

        [Required] [MaxLength(7500)] public string? Risk_Drivers_TXT { get; set; }
        [AeLabel(placeholder: "What is the level of risk (impact 1-5 x likelihood 1-5) that this risk poses at this time to the department?")]
        
        [MaxLength(7500)] public string? Residual_Risk_Level_TXT { get; set; }
        [AeLabel(placeholder: "What is the level of risk that would be optimal, or that the department/ program should aim for")]
        
        [MaxLength(7500)] public string? Target_Risk_Level_TXT { get; set; }
        //[AeLabel(placeholder: "If this risk has been monitored over time, please use the dropdown list to identify how the risk is trending at this time from previous monitoring intervals")]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        [MaxLength(7500)] public string? Risk_Trend_TXT { get; set; }
        [AeLabel(placeholder: "Please identify all mitigations and their monitoring activities used to track changes in the risk (please also indicate the intervals at which this monitoring happens ie. quarterly annually, etc.)")]

        [Required] [MaxLength(7500)] public string? Ongoing_Monitoring_Activities_TXT { get; set; }
        [AeLabel(placeholder: "Please identify all mitigations and their monitoring activities used to track changes in the risk (please also indicate the intervals at which this monitoring happens ie. quarterly annually, etc.)")]
        [MaxLength(7500)] public string? Ongoing_Monitoring_Activities_TXT2 { get; set; }
        [AeLabel(placeholder: "Please identify all mitigations and their monitoring activities used to track changes in the risk (please also indicate the intervals at which this monitoring happens ie. quarterly annually, etc.)")]
        [MaxLength(7500)] public string? Ongoing_Monitoring_Activities_TXT3 { get; set; }
        [AeLabel(placeholder: "Please identify all mitigations and their monitoring activities used to track changes in the risk (please also indicate the intervals at which this monitoring happens ie. quarterly annually, etc.)")]
        [MaxLength(7500)] public string? Ongoing_Monitoring_Activities_TXT4 { get; set; }
        [AeLabel(placeholder: "Please identify all mitigations and their monitoring activities used to track changes in the risk (please also indicate the intervals at which this monitoring happens ie. quarterly annually, etc.)")]
        [MaxLength(7500)] public string? Ongoing_Monitoring_Activities_TXT5 { get; set; }
        [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk")]
        [Required] [MaxLength(7500)] public string? Future_Mitigation_Activities_TXT { get; set; }
        [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk")]
        [MaxLength(7500)] public string? Future_Mitigation_Activities_TXT2 { get; set; }
        [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk")]
        [MaxLength(7500)] public string? Future_Mitigation_Activities_TXT3 { get; set; }
        [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk")]

        [MaxLength(7500)] public string? Future_Mitigation_Activities_TXT4 { get; set; }
        [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk")]
        [MaxLength(7500)] public string? Future_Mitigation_Activities_TXT5 { get; set; }
        //[AeLabel(placeholder: "From the drop down list, please select the corporate priority that this risk most relates to (if applicable)")]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        [MaxLength(7500)] public string? Relevant_Corporate_Priorities_TXT {get;set;}
        //[AeLabel(placeholder: "From the drop down list, please select the existing corporate risk area that this risk relates to (if applicable)")]
        [AeLabel(isDropDown: true, placeholder: "Please Select")]
        [MaxLength(7500)] public string? Relevant_Corporate_Risks_TXT {get;set;}
        [AeLabel(placeholder: "Please provide comments here as necessray to supplement any of the information requested")]
        
        [MaxLength(7500)] public string? Comments_TXT { get; set; }
        
        public PIP_Tombstone PIP_Tombstone { get; set; }

        [AeFormIgnore]
        public string Last_Updated_UserId { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }

        public List<PIP_IndicatorRisks> PIP_IndicatorRisks { get; set; }
    }
}
