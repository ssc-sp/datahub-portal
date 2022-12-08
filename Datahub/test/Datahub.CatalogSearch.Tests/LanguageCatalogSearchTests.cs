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
            engine.AddDocument("1", content, content);

            var expected = "1";
            var actual = engine.SearchDocuments(content, 10).FirstOrDefault();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoComplete_ReturnsExpected()
        {
            LanguageCatalogSearch engine = new("en");
            engine.AddDocument("1", "annual reports catalogues directories wood form descriptors", "");
            engine.AddDocument("1", "corporate management and services sector", "");
            engine.FlushIndexes();

            var autocompletes = engine.GetAutocompleteSuggestions("annual reports catalogues directories", 10).ToList();

            var expected = "annual reports catalogues directories wood";
            var actual = autocompletes[0];
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Search_ForContentField_ReturnsExpected()
        {
            var content = "This is a sample document";

            LanguageCatalogSearch engine = new("en");
            engine.AddDocument("1", "unknown title", content);

            var expected = "1";
            var actual = engine.SearchDocuments(content, 10).FirstOrDefault();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Search_ForTitleField_ReturnsExpected()
        {
            var title = "This is a sample document title";
            var content = "uknown content";

            LanguageCatalogSearch engine = new("en");
            engine.AddDocument("1", title, content);

            var expected = "1";
            var actual = engine.SearchDocuments("sample document title", 10).FirstOrDefault();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Search_ForAnyField_ReturnsNothing()
        {
            var title = "This is a sample document title";
            var content = "This is a sample document";

            LanguageCatalogSearch engine = new("en");
            engine.AddDocument("1", title, content);

            var actual = engine.SearchDocuments("nothing to be found", 10).FirstOrDefault();
            Assert.Null(actual);
        }
    }
}