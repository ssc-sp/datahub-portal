using System;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.Metadata
{
    public class MetadataDefinition
    {
        readonly Dictionary<string, FieldDefinition> _fields;
        readonly bool _ignoreDuplicates;

        public MetadataDefinition(bool ignoreDuplicates = true)
        {
            _fields = new Dictionary<string, FieldDefinition>(32);
            _ignoreDuplicates = ignoreDuplicates;
        }

        public void Add(FieldDefinition field)
        {
            if (field == null)
                throw new ArgumentException("Null field definition!");

            if (string.IsNullOrEmpty(field?.Id))
                throw new ArgumentException("Field with empty id!");

            var key = field.Id.ToLower();
            if (_fields.ContainsKey(key))
            {
                if (_ignoreDuplicates)
                    return;

                throw new ArgumentException($"Duplicated field '{field.Id}' detected!");
            }

            _fields.Add(key, field);
        }

        public FieldDefinition Get(string fieldName) => _fields.TryGetValue(GetKey(fieldName), out FieldDefinition value) ? value : null;
        
        public IEnumerable<FieldDefinition> Fields => _fields.Values.OrderBy(f => f.SortOrder);

        static string GetKey(string fieldName) => (fieldName ?? string.Empty).ToLower();
    }
}
