using Markdig.Syntax;
using Markdig;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Markdig.Syntax.Inlines;
using Datahub.Core.Services.ProjectTools;
using System.Runtime.Caching;

namespace Datahub.Tests.ProjetTools
{

    public class GithubDiscoveryTest
    {
        private RepositoryContentsClient contentClient;
        private List<RepositoryDescriptorErrors> errors;
        private readonly GitHubToolsService service;

        public GithubDiscoveryTest()
        {
            var c = new Connection(new ProductHeaderValue("datahub"));
            var connection = new ApiConnection(c);
            contentClient = new RepositoryContentsClient(connection);
            errors = new List<RepositoryDescriptorErrors>();
            service = new GitHubToolsService(new MemoryCache());
        }

        [Fact]
        public async Task TestListModulesFromGitHub()
        {
            //main/modules/azure-databricks/datahub.readme.md
            //main/modules/azure-databricks/datahub.parameters.json
            //main/modules/azure-databricks/datahub.template.json
            //var github = new GitHubClient(new ProductHeaderValue("ssc-datahub"));
            //var repo = await github.Repository.Get(GitHubOwner,GitHubRepo);
            //Assert.NotNull(repo);
            var data = await contentClient.GetAllContentsByRef(GitHubToolsService.GitHubOwner, GitHubToolsService.GitHubRepo, GitHubToolsService.GitHubModuleFolder, GitHubToolsService.GitHubBranchName);//, 
            Assert.NotNull(data);
            var modules = data.Where(d => d.Type == ContentType.Dir).ToList();
            Assert.True(modules.Count >= 4);
        }

        [Fact]
        public async Task TestCheckIfValidModule()
        {
            var c = new Connection(new ProductHeaderValue("ssc-datahub"));
            var connection = new ApiConnection(c);
            contentClient = new RepositoryContentsClient(connection);
            var data = await contentClient.GetAllContentsByRef(GitHubToolsService.GitHubOwner, GitHubToolsService.GitHubRepo, GitHubToolsService.GitHubModuleFolder, GitHubToolsService.GitHubBranchName);//, 
            Assert.NotNull(data);
            var folders = data.Where(d => d.Type == ContentType.Dir).ToList();
            Assert.True(folders.Count >= 4);
            var modules = await folders.ToAsyncEnumerable().SelectAwait(async dir => await service.GetGitHubModule(dir)).Where(d => d != null).ToListAsync();
            Assert.True(modules.Count >= 0);
            Assert.Equal(10, errors.Count);
            Assert.Contains(errors, e1 => e1.Path == "modules/azure-databricks-resource" && e1.Error == "Module azure-databricks-resource is missing 'datahub.readme.md' file");
        }
    }    
}
