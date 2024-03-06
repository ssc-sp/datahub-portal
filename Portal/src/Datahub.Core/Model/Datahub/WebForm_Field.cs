using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Elemental.Components;

namespace Datahub.Core.Model.Datahub;

public class WebFormField
{
    [Key]
    [AeFormIgnore]
    public int FieldID { get; set; }

    [StringLength(100)]
    [AeLabel("Section")]
    public string SectionDESC { get; set; }

    [Required]
    [StringLength(100)]
    [AeLabel("Field")]
    public string FieldDESC { get; set; }

    [AeLabel(label: "Description", placeholder: "Type a description to be used as a field placeholder.")]
    public string DescriptionDESC { get; set; }

    [AeLabel(label: "Choices", placeholder: "Type field choices separated by a pipe | character.")]
    public string ChoicesTXT { get; set; }

    [Required]
    [StringLength(8)]
    [AeLabel(label: "Extension", isDropDown: true, validValues: new[]
    {
        "NONE", "AMT", "AMTL", "AMTR", "CD", "CNT", "DT", "DESC", "DUR", "URL", "EMAIL", "NT", "FCTR", "ID", "FLAG",
        "MULT", "NAME", "NUM", "PCT", "QTY", "RT", "RTO", "SID", "TXT", "IND", "TIME", "TS", "VAL"
    })]
    public string ExtensionCD { get; set; } = "NONE";

    [AeFormIgnore]
    public string ExtensionLabel => ExtensionTypeReference.ClassWords[ExtensionCD];

    [Required]
    [StringLength(8)]
    [AeLabel(label: "Field Type", isDropDown: true, validValues: new[]
    {
        "Text", "Integer", "Decimal", "Boolean", "Dropdown", "Date", "Time", "Money"
    })]
    public string TypeCD { get; set; } = "Text";

    [AeLabel("Max Length")]
    public int? MaxLengthNUM { get; set; }

    [AeLabel("Notes")]
    public string NotesTXT { get; set; }

    [Required]
    [AeLabel("Mandatory")]
    public bool MandatoryFLAG { get; set; }

    [Required]
    [AeFormIgnore]
    public DateTime DateUpdatedDT { get; set; }

    public WebForm WebForm { get; set; }

    [ForeignKey("WebForm")]
    [AeFormIgnore]
    public int WebFormID { get; set; }

    public WebFormField Clone()
    {
        return new WebFormField()
        {
            SectionDESC = this.SectionDESC,
            FieldDESC = this.FieldDESC,
            DescriptionDESC = this.DescriptionDESC,
            ExtensionCD = this.ExtensionCD,
            MaxLengthNUM = this.MaxLengthNUM,
            TypeCD = this.TypeCD,
            NotesTXT = this.NotesTXT,
            MandatoryFLAG = this.MandatoryFLAG,
            ChoicesTXT = this.ChoicesTXT
        };
    }

    public void TakeValuesFrom(WebFormField other)
    {
        this.SectionDESC = other.SectionDESC;
        this.FieldDESC = other.FieldDESC;
        this.DescriptionDESC = other.DescriptionDESC;
        this.ExtensionCD = other.ExtensionCD;
        this.MaxLengthNUM = other.MaxLengthNUM;
        this.TypeCD = other.TypeCD;
        this.NotesTXT = other.NotesTXT;
        this.MandatoryFLAG = other.MandatoryFLAG;
        this.ChoicesTXT = other.ChoicesTXT;
    }
}