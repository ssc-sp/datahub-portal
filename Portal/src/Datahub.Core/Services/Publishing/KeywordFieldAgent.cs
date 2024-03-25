using Datahub.Metadata.Model;

namespace Datahub.CKAN.Package;

internal class KeywordFieldAgent : FieldAgent
{
    private const string KeywordPrefix = "keywords_";

    private readonly Dictionary<string, string[]> languages = new();

    public override bool Matches(FieldDefinition definition)
    {
        return definition.Field_Name_TXT.StartsWith(KeywordPrefix, StringComparison.InvariantCulture);
    }

    public override (bool Append, FieldAgent Agent) Instantiate(string fieldName, string fieldValue)
    {
        var append = languages.Count == 0;

        var language = fieldName.Substring(KeywordPrefix.Length);
        languages[language] = fieldValue.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(k => k.Trim()).ToArray();

        return (append, this);
    }

    public override void RenderField(IDictionary<string, object> data)
    {
        data["keywords"] = languages;
    }
}