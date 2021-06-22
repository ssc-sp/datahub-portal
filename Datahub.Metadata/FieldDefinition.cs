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

        public string Id { get; set; }
        public int SortOrder { get; set; }
        public string NameEnglish { get; set; }
        public string NameFrench { get; set; }
        public string DescriptionEnglish { get; set; }
        public string DescriptionFrench { get; set; }
        public string ObligationEnglish { get; set; }
        public string ObligationFrench { get; set; }
        public List<FieldChoice> Choices { get; set; }

        public string Name => IsFrenchCulture() ? NameFrench : NameEnglish;
        public string Description => IsFrenchCulture() ? DescriptionFrench : DescriptionEnglish;
        public string Obligation => IsFrenchCulture() ? ObligationFrench : ObligationEnglish;
        public bool IsMandatory => "Mandatory".Equals(ObligationEnglish, StringComparison.InvariantCultureIgnoreCase);
        public bool HasChoices => Choices?.Count > 0;

        static bool IsFrenchCulture() => CultureInfo.CurrentCulture.Name.StartsWith("fr", StringComparison.InvariantCulture);
    }
}
