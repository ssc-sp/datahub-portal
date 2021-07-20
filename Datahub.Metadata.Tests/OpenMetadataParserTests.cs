using System.IO;
using Xunit;

namespace NRCan.Datahub.Metadata.Tests
{
    public class OpenMetadataParserTests
    {
        [Fact]
        public void Parse_OpenMetadata_ShouldLoadMetadataFieldDefinitions()
        {
            var data = GetCvsFileContent("GCopenmetadata-GCmetadonneesouvertes.csv");
            Assert.NotNull(data);

            var definitions = OpenMetadataParser.Parse(data, ignoreDuplicateDefinitions: true);
            Assert.NotNull(definitions);
        }

        [Fact]
        public void Parse_OpenMetadata_ShouldContainExpectedFieldsDespiteTheNameCasing()
        {
            var data = GetCvsFileContent("GCopenmetadata-GCmetadonneesouvertes.csv");
            Assert.NotNull(data); 

            var definitions = OpenMetadataParser.Parse(data, ignoreDuplicateDefinitions: true);
            Assert.NotNull(definitions);

            var testFields = "restrictions|audience|type|COLLECTION|Maintainer_Email|contact_Information.en|contact_information.fr|contributor_en|contributor_fr|title_en|title_fr";

            foreach (var fieldName in testFields.Split('|'))
            {
                var field = definitions.Get(fieldName);
                Assert.NotNull(field);
            }
        }

        static string GetCvsFileContent(string fileName) => File.ReadAllText($"./Data/{fileName}");
    }
}
