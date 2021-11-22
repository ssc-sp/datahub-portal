using Datahub.Metadata.Model;
using System;
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

        public bool ValidateRequired(Func<FieldDefinition, bool> isRequired)
        {
            var map = this.ToDictionary(fv => fv.FieldDefinitionId);

            var findValidValue = (int id) => map.TryGetValue(id, out ObjectFieldValue value) && !string.IsNullOrEmpty(value.Value_TXT);
            var passRequired = (FieldDefinition f) => !isRequired.Invoke(f) || findValidValue(f.FieldDefinitionId);

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
