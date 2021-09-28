using NRCan.Datahub.Metadata.Model;
using System.Collections.Generic;

namespace NRCan.Datahub.Metadata.CKAN
{
    abstract class FieldAgent
    {
        public abstract bool Matches(FieldDefinition definition);
        public abstract (bool append, FieldAgent agent) Instanciate(string fieldName, string fieldValue);
        public abstract void RenderField(IDictionary<string, object> data);
    }
}
