using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NRCan.Datahub.Metadata.Model
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
        public virtual ICollection<FieldChoice> Choices { get; set; }
        public virtual ICollection<ObjectFieldValue> FieldValues { get; set; }

        #region Entity extensions

        public string Name => IsFrenchCulture() ? Name_French_TXT : Name_English_TXT;
        public string Description => IsFrenchCulture() ? French_DESC : English_DESC;
        public bool HasChoices => Choices?.Count > 0;
        public bool IsDateField => (Validators_TXT ?? "").Split(' ').Contains("isodate");
        static bool IsFrenchCulture() => CultureInfo.CurrentCulture.Name.StartsWith("fr", StringComparison.InvariantCulture);
        
        #endregion
    }
}
