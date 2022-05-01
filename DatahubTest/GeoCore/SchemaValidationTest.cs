using Datahub.GeoCore.Service;
using System.IO;
using Xunit;

namespace Datahub.Tests.GeoCore
{
    public class SchemaValidationTest
    {
        [Fact]
        public void ValidateSchema_WidthValidDataset_MustPass()
        {
            var json = GetFileContent("geocore_valid_dataset.json");

            var result = ShemaValidatorUtil.Validate(json);
            
            Assert.NotNull(result);
            Assert.True(result.Valid);
        }

        [Fact]
        public void ValidateSchema_WidthInvalidDataset_MustNotPass()
        {
            var json = GetFileContent("geocore_invalid_dataset.json");

            var result = ShemaValidatorUtil.Validate(json);

            Assert.NotNull(result);
            Assert.False(result.Valid);
        }

        static string GetFileContent(string fileName) => File.ReadAllText($"./Data/{fileName}");
    }
}
