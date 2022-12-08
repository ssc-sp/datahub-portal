using Markdig.Syntax;
using Markdig;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace Datahub.Core.Services.ProjectTools
{
    #nullable enable

    public record GitHubModule(string Name, string Path, string? CalculatorPath, List<GitHubModuleDescriptor> Descriptors);    

    public record GitHubModuleDescriptor(string Language, string Title, string? Description, string[]? Tags);

    public record RepositoryDescriptorErrors(string Path, string Error);

    public class GitHubToolsService
    {
        public const string GitHubOwner = "ssc-sp";
        public const string GitHubRepo = "datahub-resource-modules";
        public const string GitHubBranchName = "main";
        public const string GitHubModuleFolder = "/modules";

        public const string GitHubRawURL = "https://raw.githubusercontent.com/ssc-sp/datahub-resource-modules/";
        private const string GitHubModuleReadme = "datahub.readme.md";

        private readonly RepositoryContentsClient contentClient;
        private readonly List<RepositoryDescriptorErrors> errors;
        private readonly MemoryCache githubCache;

        public GitHubToolsService()
        {            
            var c = new Connection(new ProductHeaderValue("datahub"));
            var connection = new ApiConnection(c);
            contentClient = new RepositoryContentsClient(connection);
            errors = new List<RepositoryDescriptorErrors>();
            this.githubCache = new MemoryCache("github");
        }

        private const string CACHE_KEY = "GitModules";

        private T AddOrGetExisting<T>(string key, Func<T> valueFactory, CacheItemPolicy? policy = null)
        {
            var newValue = new Lazy<T>(valueFactory);
            var oldValue = githubCache.AddOrGetExisting(key, newValue, policy ?? new CacheItemPolicy()) as Lazy<T>;
            try
            {
                return (oldValue ?? newValue).Value;
            }
            catch
            {
                // Handle cached lazy exception by evicting from cache. Thanks to Denis Borovnev for pointing this out!
                githubCache.Remove(key);
                throw;
            }
        }

        public async List<GitHubModule> GetAllModules()
        {
            return AddOrGetExisting(CACHE_KEY, () =>
            {
                var data = await contentClient.GetAllContentsByRef(GitHubToolsService.GitHubOwner, GitHubToolsService.GitHubRepo, GitHubToolsService.GitHubModuleFolder, GitHubToolsService.GitHubBranchName);//, 
                var folders = data.Where(d => d.Type == ContentType.Dir).ToList();
                var modules = await folders.ToAsyncEnumerable().SelectAwait(async dir => await service.GetGitHubModule(dir)).Where(d => d != null).ToListAsync();
                return modules;
            }, new CacheItemPolicy() {  AbsoluteExpiration = DateTime.Now.AddHours(4)});
        }

        internal async Task<GitHubModule?> GetGitHubModule(RepositoryContent dir)
        {            
            var content = await contentClient.GetAllContentsByRef(GitHubToolsService.GitHubOwner, GitHubToolsService.GitHubRepo, dir.Path, GitHubToolsService.GitHubBranchName);//, 
            var readme = content.Where(c => c.Name.ToLower() == GitHubToolsService.GitHubModuleReadme).FirstOrDefault();
            if (readme is null)
            {
                errors.Add(new RepositoryDescriptorErrors(
                    dir.Path,
                    $"Module {dir.Name} is missing '{GitHubModuleReadme}' file"
                ));
                return null;
            }
            var readmeContent = await contentClient.GetRawContentByRef(GitHubOwner, GitHubRepo, readme.Path, GitHubBranchName);
            var readmeDoc = Markdown.Parse(Encoding.UTF8.GetString(readmeContent!));
            var readmeDocFlattened = readmeDoc.Descendants().ToList();
            var english = GetSubSections(readmeDocFlattened, "English", 1);
            var descriptors = ExtractSubSections(english, 2);
            var french = GetSubSections(readmeDocFlattened, "Français", 1);
            var descriptors_fr = ExtractSubSections(french, 2);
            //Language            

            var en = new GitHubModuleDescriptor(
                "en",
                ValidateKey("Title", dir, descriptors) ?? "Missing Title",
                ValidateKey("Why", dir, descriptors),
                ValidateKey("Tags", dir, descriptors)?.Split(",").Select(t => t.Trim()).ToArray()
            );
            var fr = new GitHubModuleDescriptor(
                "fr",
                ValidateKey("Titre", dir, descriptors_fr) ?? "Titre absent",
                ValidateKey("Pourquoi", dir, descriptors_fr),
                ValidateKey("Mots Clefs", dir, descriptors_fr)?.Split(",").Select(t => t.Trim()).ToArray()                
            );
            return new GitHubModule(dir.Name,
                dir.Path,
                ValidateKey("Calculator", dir, descriptors),
                new List<GitHubModuleDescriptor>() { en, fr }
            );
        }
        private string? ValidateKey(string key, RepositoryContent module, Dictionary<string, string> dic)
        {
            if (dic.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                errors.Add(new RepositoryDescriptorErrors(module.Path,
                    $"Module {module.Name} is missing the '{key}' section"
                ));
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
        private Dictionary<string, string> ExtractSubSections(List<MarkdownObject> readmeDocFlattened, int level)
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
                    var value = paragraph!.Inline?.FirstChild?.ToString();
                    if (value != null)
                    {
                        result[currentHeader] = value;
                    }
                    currentHeader = null;
                }
            }
            return result;
        }

    }
    
}
