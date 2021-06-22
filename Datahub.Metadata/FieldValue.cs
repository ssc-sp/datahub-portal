using System.Collections.Generic;

namespace NRCan.Datahub.Metadata
{
    public class FieldValue
    {
        public string DefinitionId { get; set; }
        public string Value { get; set; }
    }

    public class FieldValues : List<FieldValue>
    {
    }
}
