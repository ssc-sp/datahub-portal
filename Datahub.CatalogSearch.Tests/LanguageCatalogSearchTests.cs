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

        [Fact]
        public void AutoComplete_ReturnsExpected()
        {
            LanguageCatalogSearch engine = new("en");
            engine.AddDocument("1", "annual reports catalogues directories wood form descriptors");
            engine.AddDocument("1", "corporate management and services sector");
            engine.FlushIndexes();

            var autocompletes = engine.GetAutocompleteSuggestions("annual reports catalogues directories", 10).ToList();

            var expected = "annual reports catalogues directories wood";
            var actual = autocompletes[0];
            Assert.Equal(expected, actual);
        }
    }
}