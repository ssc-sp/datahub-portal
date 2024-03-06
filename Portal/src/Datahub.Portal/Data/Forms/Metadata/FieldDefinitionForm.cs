using Elemental.Components;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Portal.Data.Forms.Metadata;

public class FieldDefinitionForm
{
    [Required]
    [StringLength(256)]
    [AeFormCategory("Name", 10)]
    public string FieldNameTXT { get; set; }

    [Required]
    [StringLength(256)]
    [AeFormCategory("Name", 10)]
    public string NameEnglishTXT { get; set; }

    [Required]
    [StringLength(256)]
    [AeFormCategory("Name", 10)]
    public string NameFrenchTXT { get; set; }

    [StringLength(256)]
    [AeFormCategory("Description", 20)]
    public string EnglishDESC { get; set; }

    [StringLength(256)]
    [AeFormCategory("Description", 20)]
    public string FrenchDESC { get; set; }

    [AeFormCategory("Other", 30)]
    public bool RequiredFLAG { get; set; }

    [AeFormCategory("Other", 30)]
    public bool MultiSelectFLAG { get; set; }

    [StringLength(256)]
    [AeFormCategory("Other", 30)]
    public string DefaultValueTXT { get; set; }

    [AeFormCategory("Other", 30)]
    public int SortOrderNUM { get; set; }

    [AeFormCategory("Other", 30)]
    [AeLabel(placeholder: "Enter choice list separated by | characters. (new definitions)")]
    public string ChoicesTXT { get; set; }
}