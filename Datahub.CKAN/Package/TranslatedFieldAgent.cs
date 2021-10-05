using NRCan.Datahub.Metadata.Model;
using System.Collections.Generic;

namespace NRCan.Datahub.CKAN.Package
{
    class TranslatedFieldAgent : FieldAgent
    {
        readonly Dictionary<string, Dictionary<string, string>> _fields = new();

        public override bool Matches(FieldDefinition definition)
        {
            var fieldName = definition.Field_Name_TXT ?? "";
            return fieldName.EndsWith("_en") || fieldName.EndsWith("_fr");
        }

        public override (bool append, FieldAgent agent) Instanciate(string fieldName, string fieldValue)
        {
            var append = _fields.Count == 0;
            var fieldNameRoot = fieldName.Substring(0, fieldName.Length - 3);
            var fieldLanguage = fieldName.Substring(fieldName.Length - 2);

            if (_fields.ContainsKey(fieldNameRoot))
            {
                _fields[fieldNameRoot][fieldLanguage] = fieldValue;
            }
            else
            {
                _fields[fieldNameRoot] = new() { [fieldLanguage] = fieldValue };
            }

            return (append, this);
        }

        public override void RenderField(IDictionary<string, object> data)
        {
            foreach (var kv in _fields)
            {
                data[kv.Key] = kv.Value;
            }
        }
    }
}
