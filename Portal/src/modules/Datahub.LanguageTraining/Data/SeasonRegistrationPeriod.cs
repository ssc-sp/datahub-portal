using Elemental.Components;
using System.ComponentModel.DataAnnotations;

namespace Datahub.LanguageTraining.Data;

public class SeasonRegistrationPeriod
{
    [Key]
    [AeFormIgnore]
    public int SeasonRegistrationPeriodID { get; set; }
    public int YearNUM { get; set; }
    public byte QuarterNUM { get; set; }
    [Required]
    public DateTime OpenDT { get; set; }
    [Required]
    public DateTime CloseDT { get; set; }
}