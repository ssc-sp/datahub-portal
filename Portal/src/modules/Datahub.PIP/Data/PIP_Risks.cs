using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Elemental.Components;

namespace Datahub.PIP.Data;

public class PIPRisks
{
    [Key]
    [AeFormIgnore]
    public int RisksID { get; set; }

    [MaxLength(20)]
    [AeFormIgnore]
    public string Year { get; set; }
    [MaxLength(20)]
    [AeFormIgnore]
    public string RiskCode { get; set; }

    [AeLabel(row: "1", column: "1")]
    [Required][MaxLength(200)] public string RiskTitle { get; set; }

    [AeLabel(placeholder: "Please provide a description of the risk including the impact on the department, gov, stakeholders etc. as well as the type of risk this is ie strategic/ operational", row: "2", column: "1")]
    [Required][MaxLength(7500)] public string RiskDescriptionTXT { get; set; }

    [AeLabel(placeholder: "Please identify here the risk area (ie. HR, complexity, climate change, program delivery, funding, administration etc.)", row: "3", column: "1")]
    [Required][MaxLength(50)] public string RiskIdTXT { get; set; }

    [Required]
    [MaxLength(200)]
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "4", column: "1")]
    public string RiskCategory { get; set; }


    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "4", column: "2")]
    [MaxLength(7500)] public string RiskTrendTXT { get; set; }

    [AeFormCategory("Risk Drivers", 5)]
    [AeLabel(placeholder: "Please write only one risk driver per field. If you have more than one driver to report, please use the optional risk driver fields.", row: "5", column: "1")]
    [Required][MaxLength(7500)] public string RiskDriversTXT { get; set; }

    [AeFormCategory("Risk Drivers", 5)]
    [AeLabel(row: "6", column: "1")]
    [MaxLength(7500)] public string RiskDriversTXT2 { get; set; }

    [AeFormCategory("Risk Drivers", 5)]
    [AeLabel(row: "7", column: "1")]
    [MaxLength(7500)] public string RiskDriversTXT3 { get; set; }
    [AeFormCategory("Risk Drivers", 5)]
    [AeLabel(row: "8", column: "1")]
    [MaxLength(7500)] public string RiskDriversTXT4 { get; set; }

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
    [AeLabel(isDropDown: true, placeholder: "Please select", row: "12", column: "1")]
    [Required][MaxLength(7500)] public string OngoingMonitoringActivitiesTXT { get; set; }

    [AeFormCategory("Ongoing Monitoring Activities", 15)]
    [AeLabel(isDropDown: true, placeholder: "Please select", row: "12", column: "2")]
    [Required][MaxLength(7500)] public string OngoingMonitoringTimeframeTXT { get; set; }

    [AeFormCategory("Ongoing Monitoring Activities", 15)]
    [AeLabel(isDropDown: true, placeholder: "Please select", row: "13", column: "1")]
    [MaxLength(7500)] public string OngoingMonitoringActivitiesTXT2 { get; set; }

    [AeFormCategory("Ongoing Monitoring Activities", 15)]
    [AeLabel(isDropDown: true, placeholder: "Please select", row: "13", column: "2")]
    [MaxLength(7500)] public string OngoingMonitoringTimeframe2TXT { get; set; }

    [AeFormCategory("Ongoing Monitoring Activities", 15)]
    [AeLabel(isDropDown: true, placeholder: "Please select", row: "14", column: "1")]
    [MaxLength(7500)] public string OngoingMonitoringActivitiesTXT3 { get; set; }

    [AeFormCategory("Ongoing Monitoring Activities", 15)]
    [AeLabel(isDropDown: true, placeholder: "Please select", row: "14", column: "2")]
    [MaxLength(7500)] public string OngoingMonitoringTimeframe3TXT { get; set; }




    [AeFormCategory("Mitigation Strategies", 20)]
    [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk", row: "15", column: "1")]
    [Required]
    [MaxLength(7500)]
    public string FutureMitigationActivitiesTXT { get; set; }

    [AeFormCategory("Mitigation Strategies", 20)]
    [Required]
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "15", column: "2")]
    public string StrategyTimeline1 { get; set; }


    [AeFormCategory("Mitigation Strategies", 20)]
    [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk", row: "16", column: "1")]
    [MaxLength(7500)] public string FutureMitigationActivitiesTXT2 { get; set; }
    [AeFormCategory("Mitigation Strategies", 20)]
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "16", column: "2")]
    public string StrategyTimeline2 { get; set; }


    [AeFormCategory("Mitigation Strategies", 20)]
    [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk", row: "17", column: "1")]
    [MaxLength(7500)] public string FutureMitigationActivitiesTXT3 { get; set; }
    [AeFormCategory("Mitigation Strategies", 20)]
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "17", column: "2")]
    public string StrategyTimeline3 { get; set; }


    [AeFormCategory("Mitigation Strategies", 20)]
    [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk", row: "18", column: "1")]
    [MaxLength(7500)] public string FutureMitigationActivitiesTXT4 { get; set; }
    [AeFormCategory("Mitigation Strategies", 20)]
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "18", column: "2")]
    public string StrategyTimeline4 { get; set; }

    [AeFormCategory("Mitigation Strategies", 20)]
    [AeLabel(placeholder: "Please indicate any new activities being undertaken to further mitigate this risk", row: "19", column: "1")]
    [MaxLength(7500)] public string FutureMitigationActivitiesTXT5 { get; set; }
    [AeFormCategory("Mitigation Strategies", 20)]
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "19", column: "2")]
    public string StrategyTimeline5 { get; set; }



    [AeFormCategory("Corporate Information", 25)]
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "20", column: "1")]
    [MaxLength(7500)] public string RelevantCorporatePrioritiesTXT { get; set; }

    [AeFormCategory("Corporate Information", 25)]
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "21", column: "1")]
    [MaxLength(7500)] public string RelevantCorporateRisksTXT { get; set; }

    [AeFormCategory("", 30)]
    [AeLabel(placeholder: "Please provide comments here as necessray to supplement any of the information requested", row: "22", column: "1")]
    [MaxLength(7500)] public string CommentsTXT { get; set; }



    [AeFormIgnore]
    [Editable(false)] public string LastUpdatedUserId { get; set; }
    [AeLabel(row: "23", column: "1")]
    [AeFormCategory("Latest Update Information", 60)]
    [NotMapped]
    [Editable(false)] public string LastUpdatedUserName { get; set; }




    [AeLabel(row: "23", column: "2")]
    [AeFormCategory("Latest Update Information", 60)]
    [Required] public DateTime DateUpdatedDT { get; set; }


    public PIPTombstone PIPTombstone { get; set; }

    [AeFormIgnore]
    public int FiscalYearId { get; set; }
    public PIPFiscalYears FiscalYear { get; set; }


    [AeFormIgnore]
    public bool IsDeleted { get; set; }

    [AeFormIgnore]
    public string UserIdWhoDeleted { get; set; }


    [AeFormIgnore]
    public DateTime LastUpdatedDT { get; set; }

    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }

    public List<PIPIndicatorRisks> PIPIndicatorRisks { get; set; }
    [AeFormIgnore]
    public string EditingUserId { get; set; }
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