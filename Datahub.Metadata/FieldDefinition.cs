using System;
using System.Collections.Generic;
using System.Globalization;

namespace NRCan.Datahub.Metadata
{
    public class FieldDefinition
    {
        public FieldDefinition()
        {
            Choices = new List<FieldChoice>();
        }

        public int Id { get; set; }
        public MetadataSource Source { get; set; }
        public string FieldName { get; set; }
        public int SortOrder { get; set; }
        public string NameEnglish { get; set; }
        public string NameFrench { get; set; }
        public string DescriptionEnglish { get; set; }
        public string DescriptionFrench { get; set; }
        public bool Required { get; set; }
        public virtual ICollection<FieldChoice> Choices { get; set; }

        #region Entity extensions

        public string Name => IsFrenchCulture() ? NameFrench : NameEnglish;
        public string Description => IsFrenchCulture() ? DescriptionFrench : DescriptionEnglish;
        public bool HasChoices => Choices?.Count > 0;
        static bool IsFrenchCulture() => CultureInfo.CurrentCulture.Name.StartsWith("fr", StringComparison.InvariantCulture);
        
        #endregion
    }

    public enum MetadataSource
    {
        OpenData        
    }
}
