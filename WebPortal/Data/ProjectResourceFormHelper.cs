using Datahub.Core.EFCore;
using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;

namespace Datahub.Portal.Data
{
    public record ProjectResourceFormParams(FieldDefinitions FieldDefinitions, MetadataProfile Profile);

    public static class ProjectResourceFormHelper
    {
        private static readonly Dictionary<string, FieldDefinitions> _defCache = new();
        private static readonly Dictionary<string, MetadataProfile> _profileCache = new();

        private static FieldDefinitions BuildStorageFieldDefinitions()
        {
            var def = new FieldDefinition()
            {
                Field_Name_TXT = ProjectResourceConstants.INPUT_PARAM_STORAGE_TYPE,
                Name_English_TXT = "Storage Type",
                Choices = new List<FieldChoice>(),
                Required_FLAG = true
            };

            def.Choices.Add(new FieldChoice() { Value_TXT = ProjectResourceConstants.STORAGE_TYPE_BLOB, Label_English_TXT = "Blob" });
            def.Choices.Add(new FieldChoice() { Value_TXT = ProjectResourceConstants.STORAGE_TYPE_GEN2, Label_English_TXT = "Gen2" });

            var fd = new FieldDefinitions();
            fd.Add(def);

            return fd;
        }

        private static FieldDefinitions GetOrCreateFieldDefinitions(string resourceType)
        {
            if (!_defCache.ContainsKey(resourceType))
            {
                _defCache[resourceType] = resourceType switch
                {
                    ProjectResourceConstants.SERVICE_TYPE_STORAGE => BuildStorageFieldDefinitions(),
                    _ => default
                };
            }
            return _defCache[resourceType];
        }

        public static FieldValueContainer BuildFieldValues(Dictionary<string, string> existingValues, string resourceType)
        {
            var fd = GetOrCreateFieldDefinitions(resourceType);
            var vals = fd.Fields.Select(f =>
            {
                var val = new ObjectFieldValue()
                {
                    FieldDefinition = f
                };
                if (existingValues.ContainsKey(f.Field_Name_TXT))
                {
                    val.Value_TXT = existingValues[f.Field_Name_TXT];
                }
                return val;
            }).ToList();

            return new(resourceType, fd, vals);
        }

        private static MetadataProfile BuildStorageProfile()
        {
            var fd = GetOrCreateFieldDefinitions(ProjectResourceConstants.SERVICE_TYPE_STORAGE);

            var profileSections = new List<MetadataSection>();
            var section = new MetadataSection()
            {
                Fields = new List<SectionField>()
            };
            profileSections.Add(section);
            section.Fields.Add(new SectionField()
            {
                FieldDefinition = fd.Get(ProjectResourceConstants.INPUT_PARAM_STORAGE_TYPE),
                Section = section
            });

            var profile = new MetadataProfile()
            {
                Name = ProjectResourceConstants.SERVICE_TYPE_STORAGE,
                Sections = profileSections
            };

            return profile;
        }

        private static MetadataProfile GetOrCreateProfile(string resourceType)
        {
            if (!_profileCache.ContainsKey(resourceType))
            {
                _profileCache[resourceType] = resourceType switch
                {
                    ProjectResourceConstants.SERVICE_TYPE_STORAGE => BuildStorageProfile(),
                    _ => default
                };
            }
            return _profileCache[resourceType];
        }

        public static ProjectResourceFormParams GetProjectResourceParams(string resourceType)
        {
            var fd = GetOrCreateFieldDefinitions(resourceType);
            var profile = GetOrCreateProfile(resourceType);

            return new(fd, profile);
        }
    }
}
