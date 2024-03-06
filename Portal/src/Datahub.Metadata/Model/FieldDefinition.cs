using Datahub.Metadata.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.Metadata.Model;

public class FieldDefinition
{
    //TODO: Add an option to exclude the blank option from dropdowns

    public FieldDefinition()
    {
        Choices = new List<FieldChoice>();
    }

    public int FieldDefinitionId { get; set; }
    public int MetadataVersionId { get; set; }
    public virtual MetadataVersion MetadataVersion { get; set; }
    public string FieldNameTXT { get; set; }
    public int SortOrderNUM { get; set; }
    public string NameEnglishTXT { get; set; }
    public string NameFrenchTXT { get; set; }
    public string EnglishDESC { get; set; }
    public string FrenchDESC { get; set; }
    public bool RequiredFLAG { get; set; }
    public bool MultiSelectFLAG { get; set; }
    public string ValidatorsTXT { get; set; }
    public bool CustomFieldFLAG { get; set; }
    public string DefaultValueTXT { get; set; }
    public int? CascadeParentId { get; set; }
    public virtual ICollection<FieldChoice> Choices { get; set; }
    public virtual ICollection<ObjectFieldValue> FieldValues { get; set; }
    public virtual ICollection<SectionField> SectionFields { get; set; }

    #region Entity extensions

    public string Name => CultureUtils.SelectCulture(NameEnglishTXT, NameFrenchTXT);
    public string Description => CultureUtils.SelectCulture(EnglishDESC, FrenchDESC);
    public bool HasChoices => Choices?.Count > 0;
    public bool IsDateField => (ValidatorsTXT ?? "").Split(' ').Contains("isodate");
    public bool IsEmailField => (ValidatorsTXT ?? "").Split(' ').Contains("email");

    public string GetChoiceTextValue(string choiceValue, bool english)
    {
        var choice = Choices.FirstOrDefault(x => x.ValueTXT == choiceValue);
        return (choice is not null) ? (english ? choice.LabelEnglishTXT : choice.LabelFrenchTXT) : String.Empty;
    }

    #endregion

    public override string ToString() => $"{FieldNameTXT}({FieldDefinitionId})";
}