using NRCan.Datahub.Metadata.Model;
using System.Collections.Generic;

namespace NRCan.Datahub.Metadata.CKAN
{
    class CatchAllFieldAgent : FieldAgent
    {
        readonly string _fieldName;
        readonly string _fieldValue;
        readonly bool _multiselect;

        public CatchAllFieldAgent(bool multiselect)
        {
            _multiselect = multiselect;
        }

        private CatchAllFieldAgent(string fieldName, string fieldValue, bool multiselect)
        {
            _fieldName = fieldName;
            _fieldValue = fieldValue;
            _multiselect = multiselect;
        }

        public override bool Matches(FieldDefinition definition) => _multiselect == definition.MultiSelect_FLAG;

        public override (bool append, FieldAgent agent) Instanciate(string fieldName, string fieldValue)
        {
            return (append: true, agent: new CatchAllFieldAgent(fieldName, fieldValue, _multiselect));
        }

        public override void RenderField(IDictionary<string, object> data)
        {
            data[_fieldName] = _multiselect ? _fieldValue.Split('|') : _fieldValue;
        }
    }
}
