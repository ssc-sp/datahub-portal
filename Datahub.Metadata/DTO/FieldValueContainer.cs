using Datahub.Metadata.Model;
using System.Collections.Generic;

namespace Datahub.Metadata.DTO
{
    /// <summary>
    /// DTO to transfer object metadata field values
    /// </summary>
    public class FieldValueContainer : List<ObjectFieldValue>
    {
        public FieldValueContainer(IEnumerable<ObjectFieldValue> values) : base()
        {
            AddRange(values);
        }
    }
}
