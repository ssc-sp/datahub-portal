using System.Collections.Generic;
using System.Linq;
using NRCan.Datahub.Metadata.Model;

namespace NRCan.Datahub.Metadata
{
    public class ObjectMetadataContext
    {
        readonly string _objectId;
        readonly MetadataDefinition _definition;
        readonly List<ObjectFieldValue> _fieldValues;

        public ObjectMetadataContext(string objectId, MetadataDefinition definition, IEnumerable<ObjectFieldValue> fieldValues)
        {
            _objectId = objectId;
            _definition = definition;
            _fieldValues = fieldValues.ToList();
        }

        public MetadataDefinition MetadataDefinition => _definition;
        public List<ObjectFieldValue> FieldValues => _fieldValues;
    }
}
