using Datahub.Markdown;
using SyncDocs;
using System.Threading.Tasks;
using Xunit;

namespace Datahub.Tests.Docs
{
    public class ExternalMdTests
    {

        public const string MDEXTLINK = """
        https://learn.microsoft.com/en-ca/azure/databricks/files/workspace-modules
        """;

        public const string MDLINK1 = """
        ## Azure Databricks for Cloud Analytics

        Databricks is a platform similar to Jupyter notebooks and enables scientists to create and share documents that include live code, equations, and other multimedia resources. Databricks integrates with cloud storage and security in your cloud account, and manages and deploys cloud infrastructure on your behalf.
        """;

        [Fact]
        public async Task GivenMarkdownIdentifyExternalPage()
        {
            Assert.True(ExternalPageMarkdown.IsExternalMarkdown(MDEXTLINK));
            Assert.False(ExternalPageMarkdown.IsExternalMarkdown(MDLINK1));
            //var lastCommit = await _service.GetLastRepoCommitTS();
            //Assert.NotNull(lastCommit);
        }

        [Fact]
        public async Task GivenStatCanURLConvertToFrench()
        {
            Assert.Equal("https://www.statcan.gc.ca/fr/afc/cours-en-ligne/qgis/2020020",
                DocTranslationService.TranslateURLs("https://www.statcan.gc.ca/en/wtc/online-lectures/qgis/2020020"));

            Assert.Equal("  - [QGIS](https://www.statcan.gc.ca/fr/afc/cours-en-ligne/qgis/2020020)",
                DocTranslationService.TranslateURLs("  - [QGIS](https://www.statcan.gc.ca/en/wtc/online-lectures/qgis/2020020)"));

        }
    }
}
