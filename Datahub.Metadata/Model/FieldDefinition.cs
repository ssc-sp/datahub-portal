using Datahub.Metadata.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.Metadata.Model
{
    public class FieldDefinition
    {
        public FieldDefinition()
        {
            Choices = new List<FieldChoice>();
        }

        public int FieldDefinitionId { get; set; }
        public int MetadataVersionId { get; set; }
        public virtual MetadataVersion MetadataVersion { get; set; }
        public string Field_Name_TXT { get; set; }
        public int Sort_Order_NUM { get; set; }
        public string Name_English_TXT { get; set; }
        public string Name_French_TXT { get; set; }
        public string English_DESC { get; set; }
        public string French_DESC { get; set; }
        public bool Required_FLAG { get; set; }
        public bool MultiSelect_FLAG { get; set; }
        public string Validators_TXT { get; set; }
        public bool Custom_Field_FLAG { get; set; }
        public string Default_Value_TXT { get; set; }
        public int? CascadeParentId { get; set; }
        public virtual ICollection<FieldChoice> Choices { get; set; }
        public virtual ICollection<ObjectFieldValue> FieldValues { get; set; }
        public virtual ICollection<SectionField> SectionFields { get; set; }

        #region Entity extensions

        public string Name => CultureUtils.SelectCulture(Name_English_TXT, Name_French_TXT);
        public string Description => CultureUtils.SelectCulture(English_DESC, French_DESC);
        public bool HasChoices => Choices?.Count > 0;
        public bool IsDateField => (Validators_TXT ?? "").Split(' ').Contains("isodate");
        
        public string GetChoiceTextValue(string choiceValue, bool english)
        {
            var choice = Choices.FirstOrDefault(x => x.Value_TXT == choiceValue);
            return (choice is not null) ? (english ? choice.Label_English_TXT : choice.Label_French_TXT) : String.Empty;
        }

        #endregion

        public override string ToString() => $"{Field_Name_TXT}({FieldDefinitionId})";
    }
}
