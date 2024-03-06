using Datahub.Metadata.Utils;

namespace Datahub.Metadata.Model;

public class FieldChoice
{
    public int FieldChoiceId { get; set; }
    public int FieldDefinitionId { get; set; }
    public virtual FieldDefinition FieldDefinition { get; set; }
    public string ValueTXT { get; set; }
    public string CascadingValueTXT { get; set; }
    public string LabelEnglishTXT { get; set; }
    public string LabelFrenchTXT { get; set; }

    #region Entity extensions

    public string Label => CultureUtils.SelectCulture(LabelEnglishTXT, LabelFrenchTXT);

    #endregion
}