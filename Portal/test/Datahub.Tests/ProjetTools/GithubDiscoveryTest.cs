using Markdig.Syntax;
using Octokit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Datahub.Tests.ProjetTools;

public record GitHubModule
{
    public string Name { get; init; }
    public string Path { get; init; }
    public string? CalculatorPath { get; init; }

    public List<GitHubModuleDescriptor> Descriptors { get; init; }
}

public record GitHubModuleDescriptor
{
    public string Language { get; set; }
    public string Title { get; init; }
    public string? Description { get; init; }
    public string[]? Tags { get; init; }
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
    public const string GitHubModuleFolder = "/modules";

    public const string GitHubRawURL = "https://raw.githubusercontent.com/ssc-sp/datahub-resource-modules/";
    private const string GitHubModuleReadme = "datahub.readme.md";
    private RepositoryContentsClient contentClient;
    private List<RepositoryDescriptorErrors> errors;

    public GithubDiscoveryTest()
    {
        var c = new Connection(new ProductHeaderValue("datahub"));
        var connection = new ApiConnection(c);
        contentClient = new RepositoryContentsClient(connection);
        errors = new List<RepositoryDescriptorErrors>();
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
        var data = await contentClient.GetAllContentsByRef(GitHubOwner, GitHubRepo, GitHubModuleFolder, GitHubBranchName);//, 
        Assert.NotNull(data);
        var modules = data.Where(d => d.Type == ContentType.Dir).ToList();
        Assert.True(modules.Count >= 4);
    }

    [Fact (Skip = "Needs to be validated")]
    public async Task TestCheckIfValidModule()
    {
        var c = new Connection(new ProductHeaderValue("ssc-datahub"));
        var connection = new ApiConnection(c);
        contentClient = new RepositoryContentsClient(connection);
        var data = await contentClient.GetAllContentsByRef(GitHubOwner, GitHubRepo, GitHubModuleFolder, GitHubBranchName);//, 
        Assert.NotNull(data);
        var folders = data.Where(d => d.Type == ContentType.Dir).ToList();
        Assert.True(folders.Count >= 4);
        var modules = await folders.ToAsyncEnumerable().SelectAwait(async dir => await GetGitHubModule(dir)).Where(d => d != null).ToListAsync();
        Assert.True(modules.Count >= 0);
        Assert.Equal(10, errors.Count);
        Assert.Contains(errors, e1 => e1.Path == "modules/azure-databricks-resource" && e1.Error == "Module azure-databricks-resource is missing 'datahub.readme.md' file");
    }

    private async Task<GitHubModule?> GetGitHubModule(RepositoryContent dir)
    {
        var content = await contentClient.GetAllContentsByRef(GitHubOwner, GitHubRepo, dir.Path, GitHubBranchName);//, 
        var readme = content.Where(c => c.Name.ToLower() == GitHubModuleReadme).FirstOrDefault();
        if (readme is null)
        {
            errors.Add(new RepositoryDescriptorErrors()
            {
                Path = dir.Path,
                Error = $"Module {dir.Name} is missing '{GitHubModuleReadme}' file"
            });
            return null;
        }
        var readmeContent = await contentClient.GetRawContentByRef(GitHubOwner, GitHubRepo, readme.Path, GitHubBranchName);
        var readmeDoc = Markdig.Markdown.Parse(Encoding.UTF8.GetString(readmeContent!));
        var readmeDocFlattened = readmeDoc.Descendants().ToList();
        var english = GetSubSections(readmeDocFlattened, "English", 1);
        var descriptors = ExtractSubSections(english, 2);
        Assert.True(descriptors.ContainsKey("Title"));
        Assert.True(descriptors.ContainsKey("Why"));
        Assert.True(descriptors.ContainsKey("Tags"));
        var french = GetSubSections(readmeDocFlattened, "Français", 1);
        var descriptors_fr = ExtractSubSections(french, 2);
        //Language            
            
        var en =  new GitHubModuleDescriptor() { Title = ValidateKey("Title", dir, descriptors)?? "Missing Title", 
            Description = ValidateKey("Why", dir, descriptors), 
            Tags = ValidateKey("Tags", dir, descriptors)?.Split(",").Select(t => t.Trim()).ToArray(),
            Language = "en"
        };
        var fr = new GitHubModuleDescriptor()
        {
            Title = ValidateKey("Titre", dir, descriptors_fr) ?? "Titre absent",
            Description = ValidateKey("Pourquoi", dir, descriptors_fr),
            Tags = ValidateKey("Mots Clefs", dir, descriptors_fr)?.Split(",").Select(t => t.Trim()).ToArray(),
            Language = "en"
        };
        return new GitHubModule() {
            Name = dir.Name,
            Path = dir.Path,
            CalculatorPath = ValidateKey("Calculator", dir, descriptors),
            Descriptors = new List<GitHubModuleDescriptor>() { en,fr}
        };
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
                result[currentHeader] = paragraph!.Inline?.FirstChild?.ToString();
                currentHeader = null;
            }
        }
        return result;
    }
}