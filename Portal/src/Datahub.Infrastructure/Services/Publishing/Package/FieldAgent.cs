using Datahub.Metadata.Model;

namespace Datahub.Infrastructure.Services.Publishing.Package;

internal abstract class FieldAgent
{
    public abstract bool Matches(FieldDefinition definition);
    public abstract (bool Append, FieldAgent Agent) Instantiate(string fieldName, string fieldValue);
    public abstract void RenderField(IDictionary<string, object> data);
}