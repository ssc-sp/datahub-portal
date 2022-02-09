using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Tests.Meta_Data
{
    public static class FieldDefinitionHelper
    {
        public static FieldDefinitions LoadDefinitions()
        {
            var defs = JsonConvert.DeserializeObject<FieldDefinition[]>(GetFileContent("open_data_definitions.json")).ToList();
            var choices = JsonConvert.DeserializeObject<FieldChoice[]>(GetFileContent("open_data_definition_choices.json"));

            var defDict = defs.ToDictionary(v => v.FieldDefinitionId, v => v);
            foreach (var c in choices)
            {
                var def = defDict[c.FieldDefinitionId];
                def.Choices ??= new List<FieldChoice>();
                def.Choices.Add(c);
            }

            FieldDefinitions fieldDefinitions = new();
            fieldDefinitions.Add(defs);

            return fieldDefinitions;
        }

        public static FieldValueContainer LoadFields(FieldDefinitions definitions)
        {
            var fieldValues = JsonConvert.DeserializeObject<ObjectFieldValue[]>(GetFileContent("field_values.json")).ToList();

            foreach (var fvalue in fieldValues)
            {
                fvalue.FieldDefinition = definitions.Get(fvalue.FieldDefinitionId);
            }

            return new FieldValueContainer("86d0d9d9-ddfc-49e3-af4b-89f94c176d1d", definitions, fieldValues);
        }

        static string GetFileContent(string fileName) => File.ReadAllText($"./Data/{fileName}");
    }
}
