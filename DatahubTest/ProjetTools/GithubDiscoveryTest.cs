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

namespace Datahub.Tests.ProjetTools
{
    public record GitHubModuleDescriptor
    {
        public string Name { get; init; }
        public string Path { get; init; }
        public string Title { get; init; }
        public string? Description { get; init; }
        public string[]? Tags { get; init; }
        public string? CalculatorPath { get; init; }
    }

    public record RepositoryDescriptorErrors
    {
        public string Path { get; init; }
        public string Error { get; init; }
    }

    public class GithubDiscoveryTest
    {
        public const string GitHubOwner = "ssc-sp";
        public const string GitHubRepo = "datahub-resource-modules";
        public const string GitHubBranchName = "main";

        public const string GitHubRawURL = "https://raw.githubusercontent.com/ssc-sp/datahub-resource-modules/";
        private RepositoryContentsClient contentClient;
        private List<RepositoryDescriptorErrors> errors;

        public GithubDiscoveryTest()
        {
            var c = new Connection(new ProductHeaderValue("ssc-datahub"));
            var connection = new ApiConnection(c);
            contentClient = new RepositoryContentsClient(connection);
            errors = new List<RepositoryDescriptorErrors>();
        }

        [Fact]
        public async Task ListModulesFromGitHub()
        {
            //main/modules/azure-databricks/datahub.readme.md
            //main/modules/azure-databricks/datahub.parameters.json
            //main/modules/azure-databricks/datahub.template.json
            //var github = new GitHubClient(new ProductHeaderValue("ssc-datahub"));
            //var repo = await github.Repository.Get(GitHubOwner,GitHubRepo);
            //Assert.NotNull(repo);
            var data = await contentClient.GetAllContentsByRef(GitHubOwner, GitHubRepo, "/modules", GitHubBranchName);//, 
            Assert.NotNull(data);
            var modules = data.Where(d => d.Type == ContentType.Dir).ToList();
            Assert.True(modules.Count >= 4);
        }

        [Fact]
        public async Task CheckIfValidModule()
        {
            var c = new Connection(new ProductHeaderValue("ssc-datahub"));
            var connection = new ApiConnection(c);
            contentClient = new RepositoryContentsClient(connection);
            var data = await contentClient.GetAllContentsByRef(GitHubOwner, GitHubRepo, "/modules", GitHubBranchName);//, 
            Assert.NotNull(data);
            var modules = data.Where(d => d.Type == ContentType.Dir).ToList();
            Assert.True(modules.Count >= 4);
            var modulesDescriptors = await data.ToAsyncEnumerable().SelectAwait(async dir => await GetGitHubModule(dir)).Where(d => d != null).ToListAsync();
            Assert.True(modulesDescriptors.Count >= 0);
        }

        private async Task<GitHubModuleDescriptor?> GetGitHubModule(RepositoryContent dir)
        {
            var content = await contentClient.GetAllContentsByRef(GitHubOwner, GitHubRepo, dir.Path, GitHubBranchName);//, 
            var readme = content.Where(c => c.Name.ToLower() == "datahub.readme.md").FirstOrDefault();
            if (readme is null)
                return null;
            var readmeContent = await contentClient.GetRawContentByRef(GitHubOwner, GitHubRepo, readme.Path, GitHubBranchName);
            var readmeDoc = Markdown.Parse(Encoding.UTF8.GetString(readmeContent!));
            var readmeDocFlattened = readmeDoc.Descendants().ToList();
            var english = GetSubSections(readmeDocFlattened, "English", 1);
            var descriptors = ExtractSubSections(english, 2);
            Assert.True(descriptors.ContainsKey("Title"));
            Assert.True(descriptors.ContainsKey("Why"));
            Assert.True(descriptors.ContainsKey("Tags"));
            return new GitHubModuleDescriptor() { Title = ValidateKey("Title", dir, descriptors)?? "Missing Title", 
                Description = ValidateKey("Why", dir, descriptors), 
                Tags = ValidateKey("Tags", dir, descriptors)?.Split(",").Select(t => t.Trim()).ToArray(), 
                Name = dir.Name, 
                Path = dir.Path };
        }

        private string? ValidateKey(string key, RepositoryContent module, Dictionary<string, string> dic)
        {
            if (dic.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                errors.Add(new RepositoryDescriptorErrors()
                {                    
                    Path = module.Path,
                    Error = $"Module {module.Name} is missing the '{key}' section"
                });
                return null;
            }
        }

        private List<MarkdownObject> GetSubSections(List<MarkdownObject> readmeDocFlattened, string value, int level)
        {
            var result = new List<MarkdownObject>();
            var blockLocated = false;
            foreach (var item in readmeDocFlattened)
            {
                if (item is HeadingBlock heading && heading.Level == level)
                {
                    if (heading.Inline?.FirstChild?.ToString() == value)
                    {
                        blockLocated = true;
                        continue;
                    }
                    else if (blockLocated)
                    {
                        break;
                    }
                }
                if (blockLocated) 
                {
                    result.Add(item);
                }
            }
            return result.Skip(1).ToList();//skip the inline block
        }

        private Dictionary<string,string> ExtractSubSections(List<MarkdownObject> readmeDocFlattened, int level)
        {
            var result = new Dictionary<string, string>();
            var iterator = readmeDocFlattened.GetEnumerator();
            string? currentHeader = null;            
            while (iterator.MoveNext())
            {
                var item = iterator.Current;                
                if (item is HeadingBlock heading)
                {
                    currentHeader = null;
                    if (heading.Inline?.FirstChild != null && heading.Level == level)
                    {
                        currentHeader = heading.Inline?.FirstChild?.ToString();
                    }
                }
                if (item is ParagraphBlock paragraph && currentHeader != null)
                {
                    result[currentHeader] = paragraph.Inline?.FirstChild?.ToString();
                    currentHeader = null;
                }
            }
            return result;
        }
    }    
}
