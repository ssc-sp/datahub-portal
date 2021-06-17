using System.IO;
using Xunit;
using Datahub.Metadata.CSV;

namespace Datahub.Metadata.Tests
{
    public class CsvMetadataParserTests
    {
        [Fact]
        public void Parse_OpenMetadata_ShouldLoadMetadataFieldDefinitions()
        {
            var data = GetCvsFileContent("GCopenmetadata-GCmetadonneesouvertes.csv");
            Assert.NotNull(data);

            var definitions = CsvMetadataParser.Parse(data, ignoreDuplicateDefinitions: true);
            Assert.NotNull(definitions);
        }

        [Fact]
        public void Parse_OpenMetadata_ShouldContainExpectedFields()
        {
            var data = GetCvsFileContent("GCopenmetadata-GCmetadonneesouvertes.csv");
            Assert.NotNull(data);

            var definitions = CsvMetadataParser.Parse(data, ignoreDuplicateDefinitions: true);
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
