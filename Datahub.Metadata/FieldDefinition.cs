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
        public List<FieldChoice> Choices { get; set; }

        public bool HasChoices => Choices?.Count > 0;
    }
}
