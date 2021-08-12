using System.Collections.Generic;
using NRCan.Datahub.Metadata.Model;

namespace NRCan.Datahub.Metadata.DTO
{
    public class ObjectMetadataContext
    {
        readonly string _objectId;
        readonly FieldDefinitions _definitions;
        readonly FieldValueContainer _fieldValues;

        public ObjectMetadataContext(string objectId, FieldDefinitions definitions, IEnumerable<ObjectFieldValue> fieldValues)
        {
            _objectId = objectId;
            _definitions = definitions;
            _fieldValues = new FieldValueContainer(fieldValues);
        }

        public string ObjectId => _objectId;
        public FieldDefinitions FieldDefinitions => _definitions;
        public FieldValueContainer FieldValues => _fieldValues;
    }
}
