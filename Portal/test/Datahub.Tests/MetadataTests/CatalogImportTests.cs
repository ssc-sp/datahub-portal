using System.IO;
using System.Linq;
using Datahub.Metadata.Catalog;
using Newtonsoft.Json;
using Xunit;

namespace Datahub.Tests.MetadataTests;

public class CatalogImportTests
{
    [Fact]
    public void CatalogImportDeserialize()
    {
        var entries = JsonConvert.DeserializeObject<CatalogEntry[]>(GetFileContent("sample_catalog.json"))!.ToList();
        Assert.NotNull(entries);
    }

    static string GetFileContent(string fileName) => File.ReadAllText($"./Data/{fileName}");
}