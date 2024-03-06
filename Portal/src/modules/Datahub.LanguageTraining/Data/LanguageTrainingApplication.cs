#nullable disable

using System.ComponentModel.DataAnnotations;
using Elemental.Components;

namespace Datahub.LanguageTraining.Data;

public class LanguageTrainingApplication
{
    [Key]
    [AeFormIgnore]
    public int ApplicationID { get; set; }


    [AeFormCategory("Employee Information", 10)]
    [Required]
    [MaxLength(200)]
    public string NRCanUsername { get; set; }
    [AeFormCategory("Employee Information", 10)]
    [Required]
    public string FirstName { get; set; }
    [AeFormCategory("Employee Information", 10)]
    [Required]
    public string LastName { get; set; }
    [AeFormCategory("Employee Information", 10)]
    [Required]
    public string EmailAddressEMAIL { get; set; }
    [AeFormCategory("Employee Information", 10)]
    [AeLabel(isDropDown: true)]
    [Required]
    public string SectorBranch { get; set; }
    [AeFormCategory("Employee Information", 10)]
    [Required]
    public string City { get; set; }
    [AeFormCategory("Employee Information", 10)]
    [AeLabel(isDropDown: true)]
    [Required]
    public string ProvinceTerritory { get; set; }
    [AeFormCategory("Employee Information", 10)]
    [AeLabel(isDropDown: true)]
    [Required]
    public string EmploymentStatus { get; set; }
    [AeFormCategory("Employee Information", 10)]
    [AeLabel(isDropDown: true)]
    [Required]
    public string IAmSeeking { get; set; }
    [AeFormCategory("Language Assessment", 20)]
    public bool CompletedLETPAssessment { get; set; }
    [AeFormCategory("Language Assessment", 20)]
    public bool LanguageTrainingSinceLETPAssessment { get; set; }
    [AeFormCategory("Language Assessment", 20)]
    [AeLabel(isDropDown: true)]
    public string LanguageTrainingProvidedBy { get; set; }

    [AeFormCategory("Language Assessment", 20)]
    [AeLabel(isDropDown: true)]
    public string LastCourseSuccessfullyCompleted { get; set; }

    [AeFormCategory("Language Assessment", 20)]
    public int CompletedTrainingYear { get; set; }

    [AeFormCategory("Language Assessment", 20)]
    [AeLabel(isDropDown: true)]
    public string CompletedTrainingSession { get; set; }

    [AeFormCategory("Language Assessment", 20)]
    public bool ReportSentToNRCanLanguageSchool { get; set; }

    [AeFormCategory("Language Assessment", 20)]
    public bool EmployeeAppointedNonImperativeBasis { get; set; }
    [AeFormCategory("Language Assessment", 20)]
    public bool EmployeeLanguageProfileRaised { get; set; }
    [AeFormCategory("Language Assessment", 20)]
    public bool EmployeeInPardp { get; set; }
    [AeFormCategory("Language Assessment", 20)]
    public bool EmployeeProfessionalDevProgram { get; set; }


    [AeFormCategory("Language Assessment", 20)]
    public bool EmployeeTalentManagementExercise { get; set; }
    [AeFormCategory("Language Assessment", 20)]
    public bool EmployeeEquityGroup { get; set; }



    [AeFormCategory("Language Assessment", 20)]

    public bool SecondLanguageEvaluationResults { get; set; }
    [AeFormCategory("Language Assessment", 20)]
    [AeLabel(isDropDown: true)]

    public string SLEResultsReading { get; set; }
    [AeFormCategory("Language Assessment", 20)]
    [AeLabel(isDropDown: true)]

    public string SLEResultsWriting { get; set; }
    [AeFormCategory("Language Assessment", 20)]
    [AeLabel(isDropDown: true)]
    public string SLEResultsOral { get; set; }


    [AeFormCategory("Language Training Application", 30)]
    [AeLabel(isDropDown: true)]
    [Required]
    public string TrainingType { get; set; }
    [AeFormCategory("Language Training Application", 30)]
    public DateTime SLETestDate { get; set; }
    [AeFormCategory("Language Training Application", 30)]
    [AeFormIgnore]
    public int YearForLanguageTraining { get; set; }
    [AeFormCategory("Language Training Application", 30)]
    [AeFormIgnore]
    public byte QuarterNUM { get; set; }
    [Editable(false)]
    [AeFormCategory("Language Training Application", 30)]
    public string SessionForLanguageTraining { get; set; }
    [AeFormCategory("Language Training Application", 30)]
    [AeLabel(isDropDown: true)]
    public string ClassForLanguageTraining { get; set; }


    [AeFormCategory("Commitment and Approval", 40)]
    [Required]
    public string DelegateManagerFirstName { get; set; }
    [AeFormCategory("Commitment and Approval", 40)]
    [Required]
    public string DelegatedManagerLastName { get; set; }
    [AeFormCategory("Commitment and Approval", 40)]
    [Required]
    public string DelegatedManagerEmail { get; set; }




    [AeFormCategory("Manager Section", 50)]
    public string ManagerFirstName { get; set; }

    [AeFormCategory("Manager Section", 50)]
    public string ManagerLastName { get; set; }

    [AeFormCategory("Manager Section", 50)]
    public string ManagerEmailAddress { get; set; }
    [Required]
    [AeFormCategory("Manager Section", 50)]
    [AeLabel(isDropDown: true)]
    public string ManagerDecision { get; set; }


    [AeFormCategory("Language School Section", 60)]
    [AeLabel(isDropDown: true)]
    public string Decision { get; set; }



    [AeFormIgnore]
    public bool ApplicationCompleteEmailSent { get; set; }

    [AeFormIgnore]
    public bool ManagerDecisionSent { get; set; }

    [AeFormIgnore]
    public bool LSUDecisionSent { get; set; }

    [AeFormIgnore]
    public string LastUpdatedUserId { get; set; }

    [AeFormIgnore]
    public DateTime LastUpdatedDT { get; set; }

    [AeFormIgnore]
    public DateTime FormSubmittedDT { get; set; }
    [AeFormIgnore]
    public DateTime ManagerDecisionDT { get; set; }
    [AeFormIgnore]
    public DateTime LanguageSchoolDecisionDT { get; set; }
    [AeFormIgnore]
    public string FormSubmittedUserId { get; set; }
    [AeFormIgnore]
    public string ManagerDecisionUserId { get; set; }
    [AeFormIgnore]
    public string LanguageSchoolDecisionUserId { get; set; }

    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }
}