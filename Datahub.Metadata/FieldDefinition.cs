using System.Collections.Generic;

namespace Datahub.Metadata
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

        public bool IsMandatory => "Mandatory".Equals(ObligationEnglish, System.StringComparison.InvariantCultureIgnoreCase);
        public bool HasChoices => Choices?.Count > 0;
    }    
}
