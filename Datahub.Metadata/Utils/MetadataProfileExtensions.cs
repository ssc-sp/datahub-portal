#nullable enable
using Datahub.Metadata.Model;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.Metadata.Utils
{
    public static class MetadataProfileExtensions
    {
        public static HashSet<int> GetRequiredFieldSet(this MetadataProfile? profile)
        {
            HashSet<int> required = new();
            if (profile != null)
            {
                foreach (var s in profile.Sections)
                {
                    foreach (var f in s.Fields.Where(f => f.Required_FLAG))
                    {
                        required.Add(f.FieldDefinitionId);
                    }
                }
            }
            return required;
        }

        public static HashSet<int> GetRequiredFieldSet(this MetadataSection section)
        {
            return new(section.Fields.Where(f => f.Required_FLAG).Select(f => f.FieldDefinitionId));
        }

        public static HashSet<int> GetNotRequiredFieldSet(this MetadataSection section)
        {
            return new(section.Fields.Where(f => !f.Required_FLAG).Select(f => f.FieldDefinitionId));
        }
    }
}
