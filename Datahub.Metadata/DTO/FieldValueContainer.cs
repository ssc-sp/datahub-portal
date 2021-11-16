using Datahub.Metadata.Model;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.Metadata.DTO
{
    /// <summary>
    /// DTO to transfer object metadata field values
    /// </summary>
    public class FieldValueContainer : List<ObjectFieldValue>
    {
        public FieldValueContainer(string objectId, FieldDefinitions definitions, IEnumerable<ObjectFieldValue> values) : base()
        {
            ObjectId = objectId;
            Definitions = definitions;
            AddRange(values);
        }

        public string ObjectId { get; init; }
        public FieldDefinitions Definitions { get; init; }
        public ObjectFieldValue this[string fieldName] => GetFieldValueByName(fieldName);

        public bool ValidateRequired()
        {
            var map = this.ToDictionary(fv => fv.FieldDefinitionId);
            var findValue = (int id) => map.ContainsKey(id) ? map[id].Value_TXT : null;
            var passRequired = (FieldDefinition f) => !f.Required_FLAG || !string.IsNullOrEmpty(findValue(f.FieldDefinitionId));
            return Definitions.Fields.All(passRequired);
        }

        public FieldValueContainer GetReadonlyCopy()
        {
            return new FieldValueContainer(ObjectId, Definitions, CloneFieldsReadonly());
        }

        private IEnumerable<ObjectFieldValue> CloneFieldsReadonly()
        {
            foreach (var fieldValue in this)
            {
                var cloned = fieldValue.Clone();
                cloned.FieldDefinition ??= Definitions.Get(fieldValue.FieldDefinitionId);
                yield return cloned;
            }
        }

        private ObjectFieldValue GetFieldValueByName(string fieldName)
        {
            var definitionId = Definitions?.Get(fieldName)?.FieldDefinitionId;
            return definitionId.HasValue ? this.FirstOrDefault(v => v.FieldDefinitionId == definitionId.Value) : null;
        }
    }
}
