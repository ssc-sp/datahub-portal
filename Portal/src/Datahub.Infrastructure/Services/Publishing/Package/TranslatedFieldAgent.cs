using Datahub.Metadata.Model;

namespace Datahub.Infrastructure.Services.Publishing.Package;

internal class TranslatedFieldAgent : FieldAgent
{
    private readonly Dictionary<string, Dictionary<string, string>> fields = new();

    public override bool Matches(FieldDefinition definition)
    {
        var fieldName = definition.Field_Name_TXT ?? string.Empty;
        return fieldName.EndsWith("_en") || fieldName.EndsWith("_fr");
    }

    public override (bool Append, FieldAgent Agent) Instantiate(string fieldName, string fieldValue)
    {
        var append = fields.Count == 0;
        var fieldNameRoot = fieldName.Substring(0, fieldName.Length - 3);
        var fieldLanguage = fieldName.Substring(fieldName.Length - 2);

        if (fields.ContainsKey(fieldNameRoot))
        {
            fields[fieldNameRoot][fieldLanguage] = fieldValue;
        }
        else
        {
            fields[fieldNameRoot] = new() { [fieldLanguage] = fieldValue };
        }

        return (append, this);
    }

    public override void RenderField(IDictionary<string, object> data)
    {
        foreach (var kv in fields)
        {
            data[kv.Key] = kv.Value;
        }
    }
}