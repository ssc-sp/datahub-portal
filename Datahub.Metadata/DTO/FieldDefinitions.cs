using System;
using System.Collections.Generic;
using System.Linq;
using Datahub.Metadata.Model;

namespace Datahub.Metadata.DTO
{
    public class FieldDefinitions
    {
        readonly Dictionary<string, FieldDefinition> _fields;
        readonly bool _ignoreDuplicates;
        private int _metadataVersionId;

        public FieldDefinitions(bool ignoreDuplicates = true)
        {
            _fields = new Dictionary<string, FieldDefinition>(32);
            _ignoreDuplicates = ignoreDuplicates;
        }

        public void Add(IEnumerable<FieldDefinition> fields)
        {
            foreach (var field in fields)
                Add(field);
        }

        public void Add(FieldDefinition field)
        {
            if (field == null)
                throw new ArgumentException("Null field definition!");

            if (string.IsNullOrEmpty(field?.Field_Name_TXT))
                throw new ArgumentException("Field with empty name!");

            if (_fields.Count != 0)
            {
                if (field.MetadataVersionId != _metadataVersionId)
                    throw new ArgumentException($"Field metadata version {field.MetadataVersionId} when expected {_metadataVersionId}!");
            }
            else
            {
                _metadataVersionId = field.MetadataVersionId;
            }

            var key = GetKey(field.Field_Name_TXT);
            if (_fields.ContainsKey(key))
            {
                if (_ignoreDuplicates)
                    return;

                throw new ArgumentException($"Duplicated field '{field.Field_Name_TXT}' detected!");
            }

            _fields.Add(key, field);
        }

        public FieldDefinition Get(string fieldName) => _fields.TryGetValue(GetKey(fieldName), out FieldDefinition value) ? value : null;

        public FieldDefinition Get(int definitionId) => _fields.Values.FirstOrDefault(d => d.FieldDefinitionId == definitionId);

        public int? MapId(string fieldName) => Get(fieldName)?.FieldDefinitionId;

        public IEnumerable<FieldDefinition> Fields => _fields.Values.OrderBy(f => f.Sort_Order_NUM);

        public int MetadataVersion => _metadataVersionId;

        static string GetKey(string fieldName) => (fieldName ?? string.Empty).ToLower();
    }
}
