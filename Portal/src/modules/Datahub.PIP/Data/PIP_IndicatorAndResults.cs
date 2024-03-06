using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Elemental.Components;

namespace Datahub.PIP.Data;

public class PIPIndicatorAndResults
{
    [Key]
    [AeFormIgnore]
    public int IndicatorAndResultID { get; set; }

    [MaxLength(20)]
    [AeFormIgnore]
    public string IndicatorCode { get; set; }

    [AeFormCategory("Fiscal Year", 1)]
    public int FiscalYearId { get; set; }

    [AeFormCategory("Indicator Status", 1)]
    [MaxLength(100)]
    [AeLabel(row: "1", column: "1", isDropDown: true, validValues: new[] { "Input Required", "DRR Results Required" })]
    public string IndicatorStatus { get; set; }

    [AeFormCategory("Indicator Details", 5)]
    [AeLabel(row: "1", column: "2", isDropDown: true, placeholder: "Please Select")]
    [MaxLength(400)]
    public string OutcomeLevelDESC { get; set; }

    [AeFormCategory("Indicator Details", 5)]
    [AeLabel(row: "1", column: "2")]
    [MaxLength(1000)]
    public string ProgramOutputOrOutcomeDESC { get; set; }

    [AeFormCategory("Indicator Details", 5)]
    [AeLabel(row: "3", column: "1")]
    [MaxLength(4000)]
    public string IndicatorDESC { get; set; }

    [AeFormCategory("Indicator Details", 5)]
    [AeLabel(row: "4", column: "1", isDropDown: true, placeholder: "Please Select")]
    [MaxLength(1000)]
    public string SourceOfIndicatorDESC { get; set; }

    [AeFormCategory("Indicator Details", 5)]
    [AeLabel(row: "4", column: "2", isDropDown: true, placeholder: "Please Select")]
    [MaxLength(1000)]
    public string SourceOfIndicator2DESC { get; set; }

    [AeFormCategory("Indicator Details", 5)]
    [AeLabel(row: "4", column: "3", isDropDown: true, placeholder: "Please Select")]
    [MaxLength(1000)]
    public string SourceOfIndicator3DESC { get; set; }

    [AeFormCategory("Indicator Details", 5)]
    [AeLabel(row: "7", column: "1", isDropDown: true, placeholder: "Please Select")]
    [MaxLength(50)]
    public string CanReportOnIndicator { get; set; }

    [AeFormCategory("Indicator Details", 5)]
    [AeLabel(row: "7", column: "2", isDropDown: true, placeholder: "Please Select")]
    [MaxLength(100)]
    public string CannotReportOnIndicator { get; set; }

    [AeFormCategory("Indicator Details", 5)]
    [AeLabel(row: "8", column: "1")]
    [MaxLength(4000)]
    public string DRFIndicatorNo { get; set; }


    [AeFormCategory("Indicator Details", 5)]
    [AeLabel(row: "9", column: "1")]
    [MaxLength(1000)]
    public string BranchOptionalDESC { get; set; }

    [AeFormCategory("Indicator Details", 5)]
    [AeLabel(row: "9", column: "2")]
    [MaxLength(1000)]
    public string SubProgram { get; set; }

    [AeFormCategory("Indicator Details", 5)]
    [AeLabel(row: "10", column: "1", isDropDown: true, placeholder: "Please Select")]
    [MaxLength(1000)]
    public string IndicatorCategoryDESC { get; set; }

    [AeFormCategory("Indicator Details", 5)]
    [AeLabel(row: "10", column: "2", isDropDown: true, placeholder: "Please Select")]
    [MaxLength(1000)]
    public string IndicatorDirectionDESC { get; set; }


    [AeLabel(row: "11", column: "1")]
    [MaxLength(4000)]
    [AeFormCategory("Methodology", 10)]
    public string IndicatorRationaleDESC { get; set; }

    [AeLabel(row: "12", column: "1")]
    [AeFormCategory("Methodology", 10)]
    [MaxLength(2000)]
    public string IndicatorCalculationFormulaNUM { get; set; }

    [AeLabel(row: "13", column: "1")]
    [AeFormCategory("Methodology", 10)]
    [MaxLength(4000)]
    public string MeasurementStrategy { get; set; }

    [AeLabel(row: "14", column: "1")]
    [AeFormCategory("Methodology", 10)]
    [MaxLength(1000)]
    public string BaselineDESC { get; set; }
    [AeLabel(row: "14", column: "2")]
    [AeFormCategory("Methodology", 10)]
    [MaxLength(100)]
    public string DateOfBaselineDT { get; set; }

    [AeLabel(row: "15", column: "1")]
    [AeFormCategory("Methodology", 10)]
    [MaxLength(4000)]
    public string NotesDefinitions { get; set; }

    [AeLabel(row: "16", column: "1")]
    [AeFormCategory("Methodology", 10)]
    [MaxLength(1000)]
    public string DataSourceDESC { get; set; }
    [AeLabel(row: "16", column: "2")]
    [AeFormCategory("Methodology", 10)]
    [MaxLength(1000)]
    public string DataOwnerNAME { get; set; }


    [AeLabel(row: "17", column: "1", isDropDown: true, placeholder: "Please Select")]
    [MaxLength(1000)]
    [AeFormCategory("Methodology", 10)]
    public string FrequencyDESC { get; set; }
    [AeLabel(row: "17", column: "2", isDropDown: true, placeholder: "Please Select")]
    [MaxLength(1000)]
    [AeFormCategory("Methodology", 10)]
    public string DataTypeDESC { get; set; }

    [AeLabel(row: "18", column: "1")]
    [MaxLength(8000)]
    [AeFormCategory("Methodology", 10)]
    public string MethodologyHowWillTheIndicatorBeMeasured { get; set; }

    [AeLabel(row: "19", column: "1", isDropDown: true, placeholder: "Please Select")]
    [MaxLength(1000)]
    [AeFormCategory("Target", 20)]
    public string TargetTypeDESC { get; set; }
    [AeLabel(row: "19", column: "2")]
    [AeFormCategory("Target", 20)]
    [MaxLength(4000)]
    public string TargetDESC { get; set; }
    [AeLabel(row: "19", column: "3")]
    [AeFormCategory("Target", 20)]
    public DateTime? DateToAchieveTargetDT { get; set; }


    [AeLabel(row: "20", column: "1")]
    [AeFormCategory("Actual Results", 30)]
    [MaxLength(500)]
    public string ResultDESC { get; set; }
    [AeLabel(row: "20", column: "2")]
    [AeFormCategory("Actual Results", 30)]
    public DateTime? DateResultCollected { get; set; }
    [AeLabel(row: "20", column: "3", isDropDown: true, placeholder: "Please Select")]
    [MaxLength(1000)]
    [AeFormCategory("Actual Results", 30)]
    public string TargetMet { get; set; }


    [AeLabel(row: "21", column: "1")]
    [MaxLength(8000)]
    [AeFormCategory("Actual Results", 30)]
    public string Explanation { get; set; }

    [MaxLength(8000)]
    [AeLabel(row: "22", column: "1")]
    [AeFormCategory("Actual Results", 30)]
    public string MidyearResults { get; set; }

    [AeFormIgnore]
    [MaxLength(8000)]
    [AeFormCategory("Actual Results", 30)]
    public string TrendRationale { get; set; }


    [AeFormIgnore]
    public bool IsDeleted { get; set; }

    [AeFormIgnore]
    public string UserIdWhoDeleted { get; set; }


    [AeFormIgnore]
    public string LastUpdatedUserId { get; set; }

    [AeLabel(row: "28", column: "1")]
    [AeFormCategory("Latest Update Information", 60)]
    [NotMapped]
    [Editable(false)] public string LastUpdatedUserName { get; set; }




    [AeLabel(row: "28", column: "2")]
    [AeFormCategory("Latest Update Information", 60)]
    [Required] public DateTime DateUpdatedDT { get; set; }


    public PIPTombstone PIPTombstone { get; set; }



    public PIPFiscalYears FiscalYear { get; set; }

    [AeFormIgnore]
    public DateTime LastUpdatedDT { get; set; }

    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }

    [AeFormIgnore]
    public bool IsMethodologyLocked { get; set; }
    [AeFormIgnore]
    public bool IsTargetLocked { get; set; }
    [AeFormIgnore]
    public bool IsActualResultsLocked { get; set; }
    [AeFormIgnore]
    public bool IsLatestUpdateLocked { get; set; }

    [AeFormIgnore]
    public bool IsIndicatorStatusLocked { get; set; }
    [AeFormIgnore]
    public bool IsIndicatorDetailsLocked { get; set; }
    [AeFormIgnore]
    public bool IsFiscalYearLocked { get; set; }
    [AeFormIgnore]
    public string EditingUserId { get; set; }
    [AeFormIgnore]
    public string SourceFileName { get; set; }
    [AeFormIgnore]
    public DateTime? SourceFileUploadDate { get; set; }
    [AeFormIgnore]
    public int DuplicateCount { get; set; }
    [AeFormIgnore]
    [MaxLength(256)]
    public string DataFactoryRunId { get; set; }


}