using System.Runtime.Caching;
using System.Text;
using System.Text.Json.Serialization;
using Datahub.ProjectTools.Utils;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Octokit;
using YamlDotNet.Serialization;

namespace Datahub.ProjectTools.Services;
#nullable enable

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GitHubModuleStatus
{
    Alpha,
    Beta,
    Stable,
}

public record GitHubModule(string Name, string Path,
    string? CardName,
    string? CalculatorPath,
    string? DocumentationUrl,
    string? ReadMorePath,
    string? Icon,
    List<GitHubModuleDescriptor> Descriptors,
    GitHubModuleStatus Status);

public record GitHubModuleDescriptor(string Language, 
    string Title, 
    string? CatalogSubtitle, 
    string? CatalogDescription, 
    string? ResourceDescription, 
    string? ActionURL,
    string? ActionDescription,
    string[]? Tags);

public record RepositoryDescriptorErrors(string Path, string Error);

public class AsyncLazy<T> : Lazy<Task<T>>
{
    public AsyncLazy(Func<T> valueFactory) :
        base(() => Task.Factory.StartNew(valueFactory))
    {
    }

    public AsyncLazy(Func<Task<T>> taskFactory) :
        base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap())
    {
    }
}

public class GitHubToolsService
{
    public const string GitHubOwner = "ssc-sp";
    public const string GitHubRepo = "datahub-resource-modules";
    public const string GitHubBranchName = "main";
    public const string GitHubTemplateFolder = "/templates";

    public const string GitHubRawURL = "https://raw.githubusercontent.com/ssc-sp/datahub-resource-modules/";
    private const string GitHubTemplateReadme = "datahub.readme.md";

    private readonly RepositoryContentsClient contentClient;
    private readonly List<RepositoryDescriptorErrors> errors;
    private readonly MemoryCache githubCache;

    public GitHubToolsService()
    {
        var c = new Connection(new ProductHeaderValue("datahub"));
        var connection = new ApiConnection(c);
        contentClient = new RepositoryContentsClient(connection);
        errors = new List<RepositoryDescriptorErrors>();
        githubCache = new MemoryCache("github");
    }

    private const string CACHE_KEY = "GitModules";

    private async Task<T> AddOrGetExistingAsync<T>(string key, Func<Task<T>> valueFactory,
        CacheItemPolicy? policy = null)
    {
        var newValue = new AsyncLazy<T>(valueFactory);
        var oldValue = githubCache.AddOrGetExisting(key, newValue, policy ?? new CacheItemPolicy()) as AsyncLazy<T>;
        try
        {
            return await (oldValue ?? newValue).Value;
        }
        catch
        {
            // Handle cached lazy exception by evicting from cache. Thanks to Denis Borovnev for pointing this out!
            githubCache.Remove(key);
            throw;
        }
    }

    public async Task<List<GitHubModule>> GetAllModules()
    {
        return await AddOrGetExistingAsync(CACHE_KEY, async () =>
        {
            var data = await contentClient.GetAllContentsByRef(GitHubOwner,
                GitHubRepo, GitHubTemplateFolder,
                GitHubBranchName); //, 
            var folders = data.Where(d => d.Type == ContentType.Dir).ToList();
            var modules = await folders.ToAsyncEnumerable()
                .SelectAwait(async dir => await GetGitHubModule(dir))
                .Where(d => d != null).Select(d => d!).ToListAsync();
            return modules;
        }, new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddHours(4) });
    }

    internal async Task<GitHubModule?> GetGitHubModule(RepositoryContent dir)
    {
        var content = await contentClient.GetAllContentsByRef(GitHubOwner, GitHubRepo, dir.Path,
            GitHubBranchName); //, 
        var readme = content.Where(c => c.Name.ToLower() == GitHubTemplateReadme).FirstOrDefault();
        if (readme is null)
        {
            errors.Add(new RepositoryDescriptorErrors(
                dir.Path,
                $"Module {dir.Name} is missing '{GitHubTemplateReadme}' file"
            ));
            return null;
        }

        var readmeContent =
            await contentClient.GetRawContentByRef(GitHubOwner, GitHubRepo, readme.Path, GitHubBranchName);
        var builder = new MarkdownPipelineBuilder();
        builder.UseYamlFrontMatter();
        var readmeDoc = Markdig.Markdown.Parse(Encoding.UTF8.GetString(readmeContent!), builder.Build());
        var readmeDocFlattened = readmeDoc.Descendants().ToList();
        var english = GetSubSections(readmeDocFlattened, "English", 1);
        var descriptors = ExtractSubSections(english, 2);
        var french = GetSubSections(readmeDocFlattened, "Français", 1);
        if (french?.Count == 0)
            french = GetSubSections(readmeDocFlattened, "Francais", 1);
        var descriptors_fr = ExtractSubSections(french, 2);

        var yaml = GetFrontMatter(readmeDoc, dir);
        //Language            

        var en = new GitHubModuleDescriptor(
            "en",
            ValidateKey("Title", dir, descriptors) ?? "Missing Title",
            ValidateKey("Catalog Subtitle", dir, descriptors),
            ValidateKey("Catalog Description", dir, descriptors),
            ValidateKey("Resource Description", dir, descriptors),
            yaml.GetValueOrDefault("actionUrlEn"),
            yaml.GetValueOrDefault("actionDescriptionEn"),
            ValidateKey("Tags", dir, descriptors)?.Split(",").Select(t => t.Trim()).ToArray()
        );
        var fr = new GitHubModuleDescriptor(
            "fr",
            ValidateKey("Titre", dir, descriptors_fr) ?? "Titre absent",
            ValidateKey("Sous-Titre du Catalogue", dir, descriptors_fr),
            ValidateKey("Description du Catalogue", dir, descriptors_fr),
            ValidateKey("Description de la Ressource", dir, descriptors_fr),
            yaml.GetValueOrDefault("actionUrlFr"),
            yaml.GetValueOrDefault("actionDescriptionFr"),
            ValidateKey("Mots Clefs", dir, descriptors_fr)?.Split(",").Select(t => t.Trim()).ToArray()
        );

        var status = yaml.GetValueOrDefault("status");
        var moduleStatus = Enum.TryParse<GitHubModuleStatus>(status, true, out var moduleStatusEnum)
            ? moduleStatusEnum
            : GitHubModuleStatus.Stable;
        
        return new GitHubModule(dir.Name,
            dir.Path,
            yaml.GetValueOrDefault("dhcard"),
            yaml.GetValueOrDefault("calculator"),
            yaml.GetValueOrDefault("documentationUrl"),
            yaml.GetValueOrDefault("readMore"),
            yaml.GetValueOrDefault("icon"),
            new List<GitHubModuleDescriptor> { en, fr },
            moduleStatus
        );
    }

    private string? ValidateKey(string key, RepositoryContent module, Dictionary<string, string> dic)
    {
        if (dic.TryGetValue(key, out var value))
        {
            return value;
        }

        errors.Add(new RepositoryDescriptorErrors(module.Path,
            $"Module {module.Name} is missing the '{key}' section"
        ));
        return null;
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

                if (blockLocated)
                {
                    break;
                }
            }

            if (blockLocated)
            {
                result.Add(item);
            }
        }

        return result.Skip(1).ToList(); //skip the inline block
    }

    private Dictionary<string, string> ExtractSubSections(List<MarkdownObject> readmeDocFlattened, int level)
    {
        var result = new Dictionary<string, string>();
        using var iterator = readmeDocFlattened.GetEnumerator();
        string? currentHeader = null;
        while (iterator.MoveNext())
        {
            var item = iterator.Current;
            switch (item)
            {
                case HeadingBlock heading:
                {
                    currentHeader = null;
                    if (heading.Inline?.FirstChild != null && heading.Level == level)
                    {
                        currentHeader = heading.Inline?.FirstChild?.ToString();
                    }

                    break;
                }
                case ParagraphBlock paragraph when currentHeader != null:
                {
                    var value = paragraph!.Inline?.FirstChild?.ToString();
                    if (value != null)
                    {
                        result[currentHeader] = value;
                    }

                    currentHeader = null;
                    break;
                }
            }
        }

        return result;
    }

    private Dictionary<string, string> GetFrontMatter(MarkdownDocument document, RepositoryContent module)
    {
        var yamlBlock = document
            .Descendants<YamlFrontMatterBlock>()
            .FirstOrDefault();
        
        if(yamlBlock is null)
        {
            errors.Add(new RepositoryDescriptorErrors(
                module.Path,
                $"Cannot find yaml front matter in '{GitHubTemplateReadme}' file"
            ));
            return new Dictionary<string, string>();
        }
        
        var yaml = yamlBlock
            .Lines
            .Lines
            .Select(x => $"{x}")
            .ToList()
            .Select(x => x.Replace("---", string.Empty))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Split(":", 2))
            .ToDictionary(x => x[0].Trim(), x => x[1].Trim());

        return yaml;
    }
}