using Datahub.Markdown;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Datahub.Tests.Docs
{
    public class ExternalMdTests
    {

        public const string MD_EXT_LINK = """
        https://learn.microsoft.com/en-ca/azure/databricks/files/workspace-modules
        """;

        public const string MD_LINK1 = """
        ## Azure Databricks for Cloud Analytics

        Databricks is a platform similar to Jupyter notebooks and enables scientists to create and share documents that include live code, equations, and other multimedia resources. Databricks integrates with cloud storage and security in your cloud account, and manages and deploys cloud infrastructure on your behalf.
        """;

        [Fact]
        public async Task GivenMarkdown_IdentifyExternalPage()
        {
            Assert.True(ExternalPageMarkdown.IsExternalMarkdown(MD_EXT_LINK));
            Assert.False(ExternalPageMarkdown.IsExternalMarkdown(MD_LINK1));
            //var lastCommit = await _service.GetLastRepoCommitTS();
            //Assert.NotNull(lastCommit);
        }
    }
}
