using Datahub.CatalogSearch.Lucene;

namespace Datahub.CatalogSearch.Tests
{
    public class LanguageCatalogSearchTests
    {
        [Fact]
        public void AddingDocument_ContentMustBeFound()
        {
            var content = "This is a sample document";

            LanguageCatalogSearch engine = new("en");
            engine.AddDocument("1", content);

            var expected = "1";
            var actual = engine.SearchDocuments(content, 10).FirstOrDefault();

            Assert.Equal(expected, actual);
        }
    }
}