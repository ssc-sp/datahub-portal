using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using NRCan.Datahub.Metadata.Model;
using Newtonsoft.Json;
using NRCan.Datahub.Metadata.DTO;
using NRCan.Datahub.Metadata.CKAN;

namespace NRCan.Datahub.Metadata.Tests
{
    public class CKANPakageBuilderTest
    {
        private FieldDefinitions _fieldDefinitions;

        public CKANPakageBuilderTest()
        {
            _fieldDefinitions = LoadDefinitions();
        }

        [Fact]
        public void PakageBuilder_GivenMetadata_MustConvertToExpectedJson()
        {
            var fieldValues = LoadFields(_fieldDefinitions);
            Assert.NotNull(fieldValues);

            PackageGenerator generator = new();

            var dict = generator.GeneratePackage(fieldValues);

            // simple field
            var expected = "NRCAN";
            Assert.Equal(dict["owner_org"].ToString(), expected);

            // keyword field exists and contains the en & fr entries
            Assert.NotNull(dict["keywords"]);
            var keywords = (Dictionary<string, string[]>)dict["keywords"];
            Assert.NotNull(keywords["en"]);
            Assert.NotNull(keywords["fr"]);

            // title translated exists and contains the en & fr entries
            Assert.NotNull(dict["title_translated"]);
            var title_translated = (Dictionary<string, string>)dict["title_translated"];
            Assert.NotNull(title_translated["en"]);
            Assert.NotNull(title_translated["fr"]);

            var json = JsonConvert.SerializeObject(dict);

            Assert.NotNull(dict);
        }

        static FieldDefinitions LoadDefinitions()
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

        static FieldValueContainer LoadFields(FieldDefinitions definitions)
        {
            var fieldValues = JsonConvert.DeserializeObject<ObjectFieldValue[]>(GetFileContent("field_values.json")).ToList();
            
            foreach (var fvalue in fieldValues)
            {
                fvalue.FieldDefinition = definitions.Get(fvalue.FieldDefinitionId);
            }

            return new FieldValueContainer(definitions, fieldValues);
        }

        static string GetFileContent(string fileName) => File.ReadAllText($"./Data/{fileName}");
    }
}
