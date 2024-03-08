using System.ComponentModel.DataAnnotations;
using MudBlazor.Forms;

namespace Datahub.Finance.Data;

public class FundCenter
{
	[Key]
	[AeFormIgnore]
	public int FundCenter_ID { get; set; }

	[AeFormCategory("Sector Information")]
	[Required]
	[Editable(false)]
	public FiscalYear FiscalYear { get; set; }

	[AeFormCategory("Sector Information")]
	[MudForm(IsDropDown = true)]
	[Editable(false)]
	[Required]
	public HierarchyLevel Sector { get; set; }

	[AeFormCategory("Sector Information")]
	[MudForm(IsDropDown = true)]
	[Required]
	[Editable(false)]
	public HierarchyLevel Branch { get; set; }

	[AeFormCategory("Sector Information")]
	[MudForm(IsDropDown = true)]
	[Required]
	[Editable(false)]
	public HierarchyLevel Division { get; set; }

	[AeFormCategory("Sector Information")]
	[Range(0, 100, ErrorMessage = "Attrition rate should be set between 0 and 100")]
	public double? AttritionRate { get; set; }

	[AeFormCategory("Sector Information")]
	[MaxLength(4000)]
	public string Description { get; set; }

	[AeFormIgnore]
	public string Last_Updated_UserId { get; set; }

	[AeFormIgnore]
	public DateTime Last_Updated_DT { get; set; }

	[AeFormIgnore]
	public string Created_UserId { get; set; }

	[AeFormIgnore]
	public DateTime Created_DT { get; set; }

	[AeFormIgnore]
	[Timestamp]
	public byte[] Timestamp { get; set; }
}