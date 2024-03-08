using Elemental.Components;
using System.ComponentModel.DataAnnotations;

namespace Datahub.LanguageTraining.Data;

public class SeasonRegistrationPeriod
{
	[Key]
	[AeFormIgnore]
	public int SeasonRegistrationPeriod_ID { get; set; }
	public int Year_NUM { get; set; }
	public byte Quarter_NUM { get; set; }
	[Required]
	public DateTime Open_DT { get; set; }
	[Required]
	public DateTime Close_DT { get; set; }
}