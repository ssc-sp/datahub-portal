using NRCan.Datahub.Metadata.Model;
using System.Collections.Generic;
using System.Linq;

namespace NRCan.Datahub.Metadata.DTO
{
    /// <summary>
    /// DTO to transfer object metadata field values
    /// </summary>
    public class FieldValueContainer : List<ObjectFieldValue>
    {
        public FieldValueContainer(FieldDefinitions definitions, IEnumerable<ObjectFieldValue> values) : base()
        {
            Definitions = definitions;
            AddRange(values);
        }

        public FieldDefinitions Definitions { get; init; }

        public ObjectFieldValue this[string fieldName] => GetFieldValueByName(fieldName);

        public bool AddOrUpdate(string fieldName, string value)
        {
            var fieldValue = GetFieldValueByName(fieldName);
            if (fieldValue == null)
            {
                var definition = Definitions.Get(fieldName);
                if (definition != null)
                {
                    var newValue = new ObjectFieldValue() { FieldDefinitionId = definition.FieldDefinitionId, Value_TXT = value };
                    Add(newValue);
                    return true;
                }
            }
            else
            {
                fieldValue.Value_TXT = value;
            }
            return false;
        }

        public bool ValidateRequired()
        {
            var map = this.ToDictionary(fv => fv.FieldDefinitionId);
            var findValue = (int id) => map.ContainsKey(id) ? map[id].Value_TXT : null;
            var passRequired = (FieldDefinition f) => !f.Required_FLAG || !string.IsNullOrEmpty(findValue(f.FieldDefinitionId));
            return Definitions.Fields.All(passRequired);
        }

        private ObjectFieldValue GetFieldValueByName(string fieldName)
        {
            var definitionId = Definitions?.Get(fieldName)?.FieldDefinitionId;
            return definitionId.HasValue ? this.FirstOrDefault(v => v.FieldDefinitionId == definitionId.Value) : null;
        }
    }
}
