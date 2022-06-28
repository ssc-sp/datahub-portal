using Datahub.Core.EFCore;
using Datahub.Core.Services;
using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using Newtonsoft.Json;

namespace Datahub.Portal.Data.ProjectResource
{
    public record ProjectResourceFormParams(FieldDefinitions FieldDefinitions, MetadataProfile Profile);

    public static class ProjectResourceFormHelper
    {
        public static FieldValueContainer BuildFieldValues(FieldDefinitions fieldDefinitions, Dictionary<string, string> existingValues, string objectId = null)
        {
            if (string.IsNullOrEmpty(objectId))
            {
                objectId = Guid.NewGuid().ToString();
            }

            var vals = fieldDefinitions.Fields.Select(f =>
            {
                var val = new ObjectFieldValue()
                {
                    FieldDefinition = f,
                    FieldDefinitionId = f.FieldDefinitionId
                };

                if (existingValues.ContainsKey(f.Field_Name_TXT))
                {
                    val.Value_TXT = existingValues[f.Field_Name_TXT];
                }

                return val;
            }).ToList();

            return new(objectId, fieldDefinitions, vals);
        }

        private static FieldDefinitions BuildFieldDefsFromDynamic(dynamic fields)
        {
            var defs = new FieldDefinitions();
            var nextId = 1;

            foreach (dynamic field in fields)
            {
                var def = new FieldDefinition()
                {
                    Field_Name_TXT = field.field_name,
                    Name_English_TXT = field.name_en,
                    Name_French_TXT = field.name_fr,
                    Required_FLAG = field.required ?? false,
                    Default_Value_TXT = field.default_value,
                    FieldDefinitionId = nextId++
                };

                if (field.choices != null)
                {
                    foreach (dynamic choice in field.choices)
                    {
                        var fc = new FieldChoice()
                        {
                            FieldChoiceId = nextId++,
                            Value_TXT = choice.value,
                            Label_English_TXT = choice.label_en,
                            Label_French_TXT = choice.label_fr,
                            FieldDefinition = def,
                            FieldDefinitionId = def.FieldDefinitionId
                        };

                        def.Choices.Add(fc);
                    }
                }

                defs.Add(def);
            }

            return defs;
        }

        private static MetadataProfile BuildProfileFromDefsAndDynamic(FieldDefinitions defs, dynamic profileDyn)
        {
            var profileSections = new List<MetadataSection>();
            var nextId = 1;

            var profile = new MetadataProfile()
            {
                Name = profileDyn.name,
                Sections = profileSections,
                ProfileId = nextId++
            };

            foreach (var secDyn in profileDyn.sections)
            {
                var section = new MetadataSection()
                {
                    SectionId = nextId++,
                    Name_English_TXT = secDyn.label_en,
                    Name_French_TXT = secDyn.label_fr,
                    Fields = new List<SectionField>(),
                    Profile = profile,
                    ProfileId = profile.ProfileId
                };

                foreach (string fieldName in secDyn.fields)
                {
                    var fd = defs.Get(fieldName);
                    var sf = new SectionField()
                    {
                        FieldDefinition = fd,
                        FieldDefinitionId = fd.FieldDefinitionId,
                        Section = section,
                        SectionId = section.SectionId
                    };
                    section.Fields.Add(sf);
                }

                profileSections.Add(section);
            }


            return profile;

        }

        public static async Task<ProjectResourceFormParams> CreateResourceInputFormParams(this RequestManagementService requestManagementService, string resourceType)
        {
            var resourceJson = await requestManagementService.GetResourceInputDefinitionJson(resourceType);
            var paramDef = BuildFormParamsFromJson(resourceJson);
            return await Task.FromResult(paramDef);
        }

        private static ProjectResourceFormParams BuildFormParamsFromJson(string inputJson)
        {
            if (string.IsNullOrEmpty(inputJson))
            {
                return default;
            }

            var anonObject = JsonConvert.DeserializeObject<dynamic>(inputJson);

            var defs = BuildFieldDefsFromDynamic(anonObject.fields);
            var profile = BuildProfileFromDefsAndDynamic(defs, anonObject.profile);

            return new(defs, profile);
        }

        public static async Task<Dictionary<string, string>> GetDefaultValues(this RequestManagementService requestManagementService, string resourceType)
        {
            var formParams = await CreateResourceInputFormParams(requestManagementService, resourceType);
            return GetDefaultValues(formParams);
        }

        public static Dictionary<string, string> GetDefaultValues(ProjectResourceFormParams formParams)
        {
            return formParams.FieldDefinitions.Fields
                .Where(f => !string.IsNullOrEmpty(f.Default_Value_TXT))
                .ToDictionary(f => f.Field_Name_TXT, f => f.Default_Value_TXT);
        }

        public static async Task RequestServiceWithDefaults(this RequestManagementService requestManagementService, Datahub_ProjectServiceRequests request)
        {
            var defaultValues = await GetDefaultValues(requestManagementService, request.ServiceType);
            if (defaultValues?.Count > 0)
            {
                await requestManagementService.RequestService(request, defaultValues);
            }
            else
            {
                await requestManagementService.RequestService(request);
            }
        }

    }
}
