using Datahub.CKAN.Package;
using Datahub.Metadata.DTO;
using Datahub.Tests.Meta_Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using Xunit;

namespace Datahub.Tests.CKAN
{
    public class CKANPakageBuilderTest
    {
        private FieldDefinitions _fieldDefinitions;

        public CKANPakageBuilderTest()
        {
            _fieldDefinitions = FieldDefinitionHelper.LoadDefinitions();
        }

        [Fact]
        public void PakageBuilder_GivenMetadata_MustConvertToExpectedJson()
        {
            var fieldValues = FieldDefinitionHelper.LoadFields(_fieldDefinitions);
            Assert.NotNull(fieldValues);

            var dict = (new PackageGenerator()).GeneratePackage(fieldValues, true );//"url_goes_here"

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

            expected = "false";
            Assert.Equal(dict["imso_approval"].ToString(), expected);

            var json = JsonConvert.SerializeObject(dict);

            Assert.NotNull(dict);
        }
    }
}
