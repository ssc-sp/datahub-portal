using Datahub.Metadata.Model;

namespace Datahub.CKAN.Package;

internal class CatchAllFieldAgent : FieldAgent
{
    private readonly string fieldName;
    private readonly string fieldValue;
    private readonly bool multiselect;

    public CatchAllFieldAgent(string fieldName, string fieldValue, bool multiselect)
    {
        this.fieldName = fieldName;
        this.fieldValue = fieldValue;
        this.multiselect = multiselect;
    }

    public override bool Matches(FieldDefinition definition) => multiselect == definition.MultiSelect_FLAG;

    public override (bool Append, FieldAgent Agent) Instantiate(string fieldName, string fieldValue)
    {
        return (Append: true, Agent: new CatchAllFieldAgent(fieldName, fieldValue, multiselect));
    }

    public override void RenderField(IDictionary<string, object> data)
    {
        data[fieldName] = multiselect ? fieldValue.Split('|') : fieldValue;
    }
}