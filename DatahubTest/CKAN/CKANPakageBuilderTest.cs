using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Datahub.Metadata.Model;
using Newtonsoft.Json;
using Datahub.Metadata.DTO;
using Datahub.CKAN.Package;

namespace Datahub.Tests.CKAN
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

            var dict = (new PackageGenerator()).GeneratePackage(fieldValues, "url_goes_here");

            var expected = fieldValues.ObjectId;

            // id and name
            Assert.Equal(dict["id"].ToString(), expected);
            Assert.Equal(dict["name"].ToString(), expected);

            // simple field
            expected = "NRCAN";
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

            // notes translated exists and contains the en & fr entries
            Assert.NotNull(dict["notes_translated"]);
            var notes_translated = (Dictionary<string, string>)dict["notes_translated"];
            Assert.NotNull(notes_translated["en"]);
            Assert.NotNull(notes_translated["fr"]);

            // resources exists
            Assert.NotNull(dict["resources"]);
            var resources = (object[])dict["resources"];
            Assert.True(resources.Length == 1);
            
            var resource1 = resources[0] as Dictionary<string, object>;
            Assert.NotNull(resource1);

            // name_translated exists and contains the en & fr entries
            Assert.NotNull(resource1["name_translated"]);
            var name_translated = (Dictionary<string, string>)resource1["name_translated"];
            Assert.NotNull(name_translated["en"]);
            Assert.NotNull(name_translated["fr"]);

            expected = "ca-ogl-lgo";
            Assert.Equal(dict["license_id"].ToString(), expected);

            expected = "true";
            Assert.Equal(dict["ready_to_publish"].ToString(), expected);

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

            return new FieldValueContainer("86d0d9d9-ddfc-49e3-af4b-89f94c176d1d", definitions, fieldValues);
        }

        static string GetFileContent(string fileName) => File.ReadAllText($"./Data/{fileName}");
    }
}
