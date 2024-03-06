using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Elemental.Components;
using Microsoft.AspNetCore.Components;

namespace Datahub.PIP.Data;

public class PIPTombstone
{

    [Key]
    [AeFormIgnore]
    public int TombstoneID { get; set; }


    [MaxLength(20)]
    [AeFormIgnore]
    public string ProgramCode { get; set; }

    [AeFormIgnore]
    //[AeLabel(row: "1", column: "1")]
    //[AeFormCategory("Program Information", 1)]
    [Editable(false)] public int FiscalYearId { get; set; }

    [AeLabel(row: "1", column: "1")]
    [AeFormCategory("Program Information", 1)]
    [MaxLength(400)] public string ProgramTitle { get; set; }
    [AeFormCategory("Program Information", 1)]
    [AeLabel(row: "1", column: "2")]
    [MaxLength(400)] public string LeadSector { get; set; }
    [AeFormCategory("Program Information", 1)]
    [AeLabel(row: "1", column: "3")]
    [MaxLength(400)] public string ProgramOfficialTitle { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "2", column: "1")]
    [AeFormCategory("Program Information", 1)]

    [MaxLength(400)]
    public string CoreResponsbilityDESC { get; set; }

    [AeLabel(size: 100, row: "3", column: "1")]
    [AeFormCategory("Program Information", 1)]
    public string ProgramInventoryProgramDescriptionURL { get; set; }

    [AeLabel(size: 100, row: "4", column: "1")]
    [AeFormCategory("Program Information", 1)]
    [MaxLength(400)]
    public string LogicModel { get; set; }



    [AeFormIgnore]
    [AeLabel(row: "6", column: "1")]
    [AeFormCategory("Spending (in $) as per GC InfoBase (to be updated by CMSS)", 20)]
    [Column(TypeName = "Money")]
    public double? PlannedSpendingAMTL { get; set; }
    [AeFormIgnore]
    [AeLabel(row: "6", column: "2", placeholder: "to be updated by CMSS")]
    [AeFormCategory("Spending (in $) as per GC InfoBase (to be updated by CMSS)", 20)]
    [Column(TypeName = "Money")]
    public double? ActualSpendingAMTL { get; set; }


    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "8", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string StrategicPriorities1DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "9", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string StrategicPriorities2DESC { get; set; }


    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "10", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string MandateLetterCommitment1DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "11", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string MandateLetterCommitment2DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "12", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string MandateLetterCommitment3DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "13", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string MandateLetterCommitment4DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "14", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string TransferPaymentPrograms1DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "15", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string TransferPaymentPrograms2DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "16", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string TransferPaymentPrograms3DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "17", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string TransferPaymentPrograms4DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "18", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string TransferPaymentPrograms5DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "19", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string TransferPaymentPrograms6DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "20", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string TransferPaymentPrograms7DESC { get; set; }
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "20", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string TransferPaymentPrograms8DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "21", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string TransferPaymentProgramsLess51DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "22", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string TransferPaymentProgramsLess52DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "23", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string TransferPaymentProgramsLess53DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "24", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string HorizontalInitiative1DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "25", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string HorizontalInitiative2DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "26", column: "1")][AeFormCategory("Program Tags to be Updated by Sector", 50)][MaxLength(400)] public string HorizontalInitiative3DESC { get; set; }

    [AeFormCategory("Program Tags to be Updated by Sector", 50)]
    [MaxLength(4000)]
    [AeLabel(placeholder: "Please Select", row: "27", column: "1")]
    public string RelatedProgramOrActivities { get; set; }






    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "28", column: "1")]
    [AeFormCategory("Program Tags as per TBS Database and GCInfoBase", 51)]

    [MaxLength(400)]
    public string DepartmentalResult1CD { get; set; }
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "28", column: "2")]
    [AeFormCategory("Program Tags as per TBS Database and GCInfoBase", 51)]
    [MaxLength(400)]
    public string DepartmentalResult2CD { get; set; }
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "28", column: "3")]
    [AeFormCategory("Program Tags as per TBS Database and GCInfoBase", 51)]
    [MaxLength(400)]
    public string DepartmentalResult3CD { get; set; }


    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "29", column: "1")][AeFormCategory("Program Tags as per TBS Database and GCInfoBase", 51)][MaxLength(400)] public string MethodOfIntervention1DESC { get; set; }
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "29", column: "2")][AeFormCategory("Program Tags as per TBS Database and GCInfoBase", 51)][MaxLength(400)] public string MethodOfIntervention2DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "30", column: "1")][AeFormCategory("Program Tags as per TBS Database and GCInfoBase", 51)][MaxLength(400)] public string TargetGroup1DESC { get; set; }
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "30", column: "2")][AeFormCategory("Program Tags as per TBS Database and GCInfoBase", 51)][MaxLength(400)] public string TargetGroup2DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "31", column: "1")][AeFormCategory("Program Tags as per TBS Database and GCInfoBase", 51)][MaxLength(400)] public string TargetGroup3DESC { get; set; }
    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "31", column: "2")][AeFormCategory("Program Tags as per TBS Database and GCInfoBase", 51)][MaxLength(400)] public string TargetGroup4DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "32", column: "1")][AeFormCategory("Program Tags as per TBS Database and GCInfoBase", 51)][MaxLength(400)] public string TargetGroup5DESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "33", column: "1")][AeFormCategory("Program Tags as per TBS Database and GCInfoBase", 51)][MaxLength(400)] public string GovernmentOfCanadaActivityTagsDESC { get; set; }

    [AeLabel(isDropDown: true, placeholder: "Please Select", row: "34", column: "1")][AeFormCategory("Program Tags as per TBS Database and GCInfoBase", 51)][MaxLength(4000)] public string CanadianClassificationOfFunctionsOfGovernmentDESC { get; set; }



    [AeLabel(isDropDown: true, placeholder: "Please Select")]
    [AeFormCategory("GBA Plus", 56)]
    [MaxLength(50)]
    public string DoesIndicatorEnableProgramMeasureEquityOption { get; set; }

    [AeFormCategory("GBA Plus", 56)]
    [AeLabel(isDropDown: true, placeholder: "Please Select", validValues: new[] { "Yes", "No" })]
    [MaxLength(10)]
    public string CollectingData { get; set; }

    [AeFormCategory("GBA Plus", 56)]
    [AeLabel(isDropDown: true, placeholder: "Please Select", validValues: new[] { "Yes", "No" })]
    [MaxLength(10)]
    public string DisaggregatedData { get; set; }


    [AeFormCategory("GBA Plus", 56)]
    [AeLabel(isDropDown: true, placeholder: "Please Select")]
    [MaxLength(1000)]
    public string IsEquitySeekingGroup { get; set; }

    [AeFormCategory("GBA Plus", 56)]
    [AeLabel(isDropDown: true, placeholder: "Please Select")]
    [MaxLength(1000)]
    public string IsEquitySeekingGroup2 { get; set; }

    [AeFormCategory("GBA Plus", 56)]
    [AeLabel(placeholder: "If others, please add comment here ")]
    [MaxLength(4000)]
    public string IsEquitySeekingGroupOther { get; set; }

    [AeFormCategory("GBA Plus", 56)]
    [MaxLength(4000)]
    public string DisaggregatedDataInformation { get; set; }




    [AeFormCategory("GBA Plus", 56)]
    [MaxLength(8000)]
    [AeFormIgnore]
    public string DoesIndicatorEnableProgramMeasureEquity { get; set; }

    [AeFormCategory("GBA Plus", 56)]
    [MaxLength(4000)]
    [AeFormIgnore]
    public string NoEquitySeekingGroup { get; set; }


    [AeFormIgnore]
    [AeLabel(row: "40", column: "1")]
    [AeFormCategory("Date of PIP Approval", 57)] public DateTime? ApprovalByProgramOfficalDT { get; set; }
    [AeFormIgnore]
    [AeLabel(row: "40", column: "2")]
    [AeFormCategory("Date of PIP Approval", 57)] public DateTime? ConsultationWithTheHeadOfEvaluationDT { get; set; }
    [AeFormIgnore]
    [AeLabel(row: "40", column: "3")]
    [AeFormCategory("Date of PIP Approval", 57)] public DateTime? FunctionalSignOffDT { get; set; }


    [AeLabel(row: "41", column: "1")]
    [AeFormCategory("Program Notes", 57)]
    [MaxLength(4000)]
    public string ProgramNotes { get; set; }



    [AeFormIgnore]
    public string LastUpdatedUserId { get; set; }

    [AeLabel(row: "42", column: "1")]
    [AeFormCategory("Latest Update Information", 60)]
    [NotMapped]
    [Editable(false)] public string LastUpdatedUserName { get; set; }



    [AeLabel(row: "42", column: "2")]
    [AeFormCategory("Latest Update Information", 60)]
    public DateTime DateUpdatedDT { get; set; }

    [NotMapped]
    [AeFormIgnore]
    public RenderFragment PowerBiUrl { get; set; }


    [AeFormIgnore]
    public DateTime LastUpdatedDT { get; set; }

    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }

    public PIPFiscalYears FiscalYear { get; set; }

    [AeFormIgnore]
    public string EditingUserId { get; set; }



    [AeFormIgnore]
    public bool IsProgramInformationLocked { get; set; }
    [AeFormIgnore]
    public bool IsSpendingLocked { get; set; }
    [AeFormIgnore]
    public bool IsSectorProgramTagsLocked { get; set; }
    [AeFormIgnore]
    public bool IsGCInfoBaseProgramTagsLocked { get; set; }
    [AeFormIgnore]
    public bool IsGBALocked { get; set; }
    [AeFormIgnore]
    public bool IsDateOfPipApprovalLocked { get; set; }
    [AeFormIgnore]
    public bool IsLatestUpdateInformationLocked { get; set; }
    [AeFormIgnore]
    public bool IsProgramNotesLocked { get; set; }
}