using NRCan.Datahub.Metadata.Model;
using System.Collections.Generic;

namespace NRCan.Datahub.CKAN.Package
{
    abstract class FieldAgent
    {
        public abstract bool Matches(FieldDefinition definition);
        public abstract (bool append, FieldAgent agent) Instantiate(string fieldName, string fieldValue);
        public abstract void RenderField(IDictionary<string, object> data);
    }
}
