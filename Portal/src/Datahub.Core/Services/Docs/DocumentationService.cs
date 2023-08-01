using Datahub.Core.Services.Wiki;
using Datahub.Shared.Annotations;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.Extensions.Http;
using Polly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Datahub.Core.Services.Docs;

public enum DocumentationGuide
{
    [StringValue("UserGuide")]
    UserGuide,
    [StringValue("AdminGuide")]
    AdminGuide,
    [StringValue("DeveloperGuide")]
    DevGuide,
    [StringValue("")]
    RootFolder
}

#nullable enable

public class DocumentationService 
{
    private readonly string _docsRoot;
    private readonly string _docsEditPrefix;
    private readonly ILogger<DocumentationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string DOCS_ROOT_CONFIG_KEY = "docsURL";
    private const string DOCS_EDIT_URL_CONFIG_KEY = "EditdocsURLPrefix";

    private IList<TimeStampedStatus> _statusMessages;
    private DocItem? enOutline;
    private DocItem? frOutline;
    private readonly IMemoryCache _cache;

    public DocumentationService(IConfiguration config, ILogger<DocumentationService> logger, 
        IHttpClientFactory httpClientFactory,
        IMemoryCache docCache)
    {
        _docsRoot = config.GetValue(DOCS_ROOT_CONFIG_KEY, "https://raw.githubusercontent.com/ssc-sp/datahub-docs/main/")!;
        _docsEditPrefix = config.GetValue(DOCS_EDIT_URL_CONFIG_KEY, "https://github.com/ssc-sp/datahub-docs/edit/main/")!;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _statusMessages = new List<TimeStampedStatus>();
        _cache = docCache;
    }

    public async Task<bool> InvalidateCache()
    {
        try
        {
            var cache = _cache as MemoryCache;
            if (cache != null)
            {
                //https://stackoverflow.com/questions/49176244/asp-net-core-clear-cache-from-imemorycache-set-by-set-method-of-cacheextensions/49425102#49425102
                //this weird trick removes all the entries
                var percentage = 1.0;//100%
                cache.Compact(percentage);

                await LoadResourceTree(DocumentationGuide.UserGuide);

                _logger.LogInformation("Document cache has been cleared");
                return true;
            }
            else
            {
                _logger.LogWarning("Could not clear the cache.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return false;
    }

    private void AddStatusMessage(string message)
    {
        var error = new TimeStampedStatus(DateTime.UtcNow, message);
        _statusMessages.Add(error);
    }

    public string BuildAbsoluteURL(string relLink)
    {
        if (relLink is null)
        {
            throw new ArgumentNullException(nameof(relLink));
        }

        return new Uri(_docsRoot + relLink).AbsoluteUri;
    }

    private static DocItem? ParseSidebar(DocumentationGuide guide, string? inputMarkdown, Func<string, string> mapId)
    {
        if (string.IsNullOrEmpty(inputMarkdown))
        {
            return default;
        }

        var doc = Markdown.Parse(inputMarkdown);

        var root = DocItem.MakeRoot(guide, GetPageCode("root"));

        ProcessBlock(doc, 0, null, root, mapId);
        return root;
    }

    private static DocItem? ProcessBlock(MarkdownObject markdownObject, int level, DocItem? currentItem, DocItem parent, Func<string, string> mapId)
    {
        switch (markdownObject)
        {
            case LiteralInline literalInline:
                var title = literalInline.ToString();
                var id = GetPageCode(title ?? "");
                if (currentItem is null)
                {
                    var docItem1 = DocItem.GetItem(parent.DocumentationGuide, id, level, title, null);
                    parent?.Children.Add(docItem1);
                    return docItem1;
                }
                else
                {
                    currentItem.Title = title;
                }
                return null;

            case LinkInline linkInline:
                //[Microservice_Architecture](/Architecture/Microservice_Architecture.md)

                var itemId = mapId.Invoke(linkInline.Url ?? "");
                var docItem = DocItem.GetItem(parent.DocumentationGuide, itemId, level, linkInline.Title, linkInline.Url);
                parent.Children.Add(docItem);

                foreach (var child in linkInline)
                {
                    ProcessBlock(child, level, docItem, parent, mapId);
                }
                return docItem;

            case LeafBlock paragraphBlock:
                if (paragraphBlock.Inline != null)
                    return ProcessBlock(paragraphBlock.Inline, level, currentItem, parent, mapId);
                break;

            case ContainerInline inline:
                DocItem? newDoc = null;
                foreach (var child in inline)
                {
                    var res = ProcessBlock(child, level, currentItem, parent, mapId);
                    if (res != null)
                        newDoc = res;
                }
                return newDoc;

            case ContainerBlock containerBlock:
                DocItem? currentParent = parent;
                var currentLevel = level;
                if (containerBlock is ListItemBlock)
                    currentLevel++;
                foreach (var child in containerBlock)
                {
                    var res2 = ProcessBlock(child, currentLevel, currentItem, 
                        (containerBlock is ListItemBlock) ? currentParent : parent, mapId);
                    if (res2 != null)
                        currentParent = res2;
                }
                return currentParent;
        }
        return null;
    }

    private string CleanupCharacters(string input)
    {
        var deAccented = new string(input?.Normalize(NormalizationForm.FormD)
            .ToCharArray()
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());

        var deSpaced = deAccented.Replace(" ", "-");

        return deSpaced;
    }

    private IList<string> GetPath(AbstractMarkdownPage resource)
    {
        if (resource is null)
        {
            return new List<string>();
        }

        var parentPath = GetPath(resource.Parent);
        parentPath.Add(CleanupCharacters(resource.Title));
        return parentPath;
    }

    public async Task BuildDocAndPreviews(DocItem doc)
    {
        if (doc.Title is not null)
        {
            doc.Content = await LoadDocsPage(DocumentationGuide.RootFolder, doc.GetMarkdownFileName());
            BuildPreview(doc);
        } else
        {
            //top level node
            doc.Content = null;
            doc.Preview = String.Join(" ,", doc.Children.Select(d => d.Title));
        }
        foreach (var item in doc.Children)
        {
            await BuildDocAndPreviews(item);
        }
    }

    private void BuildPreview(DocItem doc)
    {
        if (string.IsNullOrEmpty(doc.Content))
        {
            doc.Preview = String.Join(", ", doc.Children.Select(d => d.Title));
            return;
        }
        var cardDoc = Markdown.Parse(doc.Content);
        var cardDocFlattened = cardDoc.Descendants();

        var firstHeading = cardDocFlattened.FirstOrDefault(e => e is HeadingBlock) as HeadingBlock;
        var firstPara = cardDocFlattened.FirstOrDefault(e => e is ParagraphBlock) as ParagraphBlock;
        if (firstHeading?.Inline?.FirstChild is null || firstPara?.Inline?.FirstChild is null)
        {
            AddStatusMessage($"Invalid card {doc.GetDescription()} - first Header or first Paragraph missing");
            doc.ContentTitle = null;
            doc.Preview = String.Join(" ,",doc.Children.Select(d => d.Title));
            return;
        }

        var title = firstHeading.Inline.FirstChild.ToString();
        var preview = firstPara.Inline.FirstChild.ToString();

        doc.ContentTitle = title;
        doc.Preview = preview;
    }

    public const string LOCALE_EN = "";
    public const string LOCALE_FR = "fr";

    private async Task LoadResourceTree(DocumentationGuide guide, bool useCache = true)
    {
        var fileMappings = await LoadDocsPage(DocumentationGuide.RootFolder, FILE_MAPPINGS, null,useCache);
        var docFileMappings = new DocumentationFileMapper(fileMappings);

        _statusMessages = new List<TimeStampedStatus>();

        AddStatusMessage("Loading resources");

        enOutline = ParseSidebar(guide, await LoadDocsPage(guide, SIDEBAR, LOCALE_EN, useCache), docFileMappings.GetEnglishDocumentId);
        if (enOutline is null)
            throw new InvalidOperationException("Cannot load sidebar and content");

        frOutline = ParseSidebar(guide, await LoadDocsPage(guide, $"{SIDEBAR}", LOCALE_FR, useCache), docFileMappings.GetFrenchDocumentId);
        if (frOutline is null)
            throw new InvalidOperationException("Cannot load sidebar and content");
        
        AddStatusMessage("Finished loading sidebars");

    }

    public DocItem? LoadPage(string id, bool isFrench)
    {
        var searchRoot = isFrench ? frOutline : enOutline;
        if (searchRoot is null)
            throw new InvalidOperationException("sidebar not loaded");
        return searchRoot.LocateID(id);
    }

    public DocItem? LoadPageFromPath(string path, bool isFrench)
    {
        var searchRoot = isFrench ? frOutline : enOutline;
        if (searchRoot is null)
            throw new InvalidOperationException("sidebar not loaded");
        return searchRoot.LocatePath(path);
    }

    public DocItem? GetParent(DocItem docItem, DocItem? currentNode = null)
    {
        if (docItem == enOutline || docItem == frOutline)
            return null;
        if (currentNode is null)
        {
            return GetParent(docItem, enOutline) ?? GetParent(docItem, frOutline);
        }
        if (currentNode.Children is null || currentNode.Children.Count == 0)
            return null;
        foreach (var item in currentNode.Children)
        {
            if (item == docItem)
                return currentNode;
            var nextLevel = GetParent(docItem, item);
            if (nextLevel != null)
                return nextLevel;
        }
        return null;
    }

    public const string SIDEBAR = "_sidebar.md";
    public const string FILE_MAPPINGS = "filemappings.json";

    private string BuildURL(DocumentationGuide guide, string? locale, string name, IList<string>? folders = null)
    {
        var allFolders = new List<string>();
        //sb.Append($"{(string.IsNullOrEmpty(locale) ? string.Empty : (locale + '/'))}{guide.GetStringValue()}/");
        if (!string.IsNullOrEmpty(locale)) allFolders.Add(locale);
        if (!string.IsNullOrEmpty(guide.GetStringValue())) allFolders.Add(guide.GetStringValue());
        if (folders != null) allFolders.AddRange(folders);

        StringBuilder sb = new();

        sb.Append(_docsRoot);
        
        if (allFolders.Count > 0)
        {
            foreach (var f in allFolders)
            {
                sb.Append($"{f}/");
            }
        }
        sb.Append(name);

        return sb.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="guide"></param>
    /// <param name="name"></param>
    /// <param name="locale">Leave empty for "en", "fr" has its own folder</param>
    /// <param name="useCache"></param>
    /// <returns></returns>
    private async Task<string?> LoadDocsPage(DocumentationGuide guide, string? name, string? locale = "", bool useCache = true)
    {
        if (name is null) return null;
        return await LoadDocs(BuildURL(guide, locale??string.Empty, name), useCache);
    }

    private const string LAST_COMMIT_TS = "LAST_COMMIT_TS";
    public const string COMMIT_API_URL = "https://api.github.com/repos/ssc-sp/datahub-docs/commits";

    public async Task<DateTime?> GetLastRepoCommitTS(bool useCache = true)
    {
        if (_cache.TryGetValue(LAST_COMMIT_TS, out DateTime? lastTS) && useCache)
            if (lastTS.HasValue) return lastTS.Value;
        var node = await ReadURL(new Dictionary<string, string>() { { "path", "UserGuide/_sidebar.md" }, { "sha", "main" }, });
        var lastCommit = (DateTime?)node?[0]?["commit"]?["author"]?["date"];
        if (lastCommit.HasValue)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                // Keep in cache for this time, reset time if accessed.
                .SetAbsoluteExpiration(DateTime.Now.AddHours(1));

            // Save data in cache.
            _cache.Set(LAST_COMMIT_TS, lastCommit.Value, cacheEntryOptions);
            return lastCommit.Value;
        }
        _logger.LogWarning($"Cannot load last commit timestamp for user docs");
        return null;
    }

    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                                                                        retryAttempt)));
    }

    public async Task<JsonNode?> ReadURL(Dictionary<string, string>? parameters = null)
    {
        var client = _httpClientFactory.CreateClient();

        var builder = new UriBuilder(new Uri(COMMIT_API_URL));
        if (parameters != null)
            builder.Query = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));


        var res = await GetRetryPolicy().ExecuteAsync(async () => {
            //builder.Query = "search=usa";
            var request = new HttpRequestMessage() { RequestUri = builder.Uri, Method = HttpMethod.Get };
            //public const string USER_AGENT = ;
            client.DefaultRequestHeaders.Add("User-Agent", "DataHub");
            return await client.SendAsync(request);
        });

        if (res.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Received status code {res.StatusCode}");
        }

        return JsonNode.Parse(await res.Content.ReadAsStreamAsync());

    }



    private async Task<string?> LoadDocs(string url, bool useCache = true, bool skipFrontMatter = true)
    {
        if (_cache.TryGetValue(url, out var docContent) && useCache)
            return docContent as string;

        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            var content = await httpClient.GetStringAsync(url);
            if (skipFrontMatter)
            {                 
                content = MarkdownHelper.RemoveFrontMatter(content);
            }
            // Set cache options.
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                // Keep in cache for this time, reset time if accessed.
                .SetAbsoluteExpiration(DateTime.Now.AddHours(1));

            // Save data in cache.
            _cache.Set(url, content, cacheEntryOptions);
            return content;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading {url}", url);
            AddStatusMessage($"Error loading {url}");

            return default(string);
        }
    }
    
    static long GetStableHashCode(string str)
    {
        unchecked
        {
            long hash1 = 5381;
            long hash2 = hash1;

            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }

    public static string GetPageCode(string url)
    {
        return GetStableHashCode(url).ToString("X").ToLowerInvariant();
    }

    public static bool CompareCulture(string c1, string c2)
    {
        var ci1 = CultureInfo.GetCultureInfo(c1);
        var ci2 = CultureInfo.GetCultureInfo(c2);
        return ci1.Equals(ci2) || ci1.Parent.Equals(ci2) || ci2.Parent.Equals(ci1);
    }

    public async Task<DocItem?> GetLanguageRoot(DocumentationGuide guide, string locale, bool useCache = true)
    {
        if (enOutline == null || frOutline == null)
        {
            await LoadResourceTree(guide, useCache);
        }

        var result = CompareCulture(locale,"fr") ? frOutline : enOutline;
        return result;
    }

    public async Task<string?> LoadResourcePage(DocItem card)
    {
        return await LoadDocsPage(DocumentationGuide.RootFolder, card.GetMarkdownFileName());
    }

    public string GetEditUrl(DocItem card) => $"{_docsEditPrefix}{card.GetMarkdownFileName()}";

    public void RemoveFromCache(DocItem item)
    {
        if (item.GetMarkdownFileName != null)
        {
            var path = BuildURL(item.DocumentationGuide, null, item.GetMarkdownFileName()!);
            _cache.Remove(path);
        }
    }

    public IReadOnlyList<TimeStampedStatus> GetErrorList() => _statusMessages.AsReadOnly();

    public void LogNotFoundError(string pageName, string resourceRoot) => AddStatusMessage($"{pageName} was not found in {resourceRoot} cache");

    public void LogNoArticleSpecifiedError(string url, string resourceRoot) => AddStatusMessage($"Embedded resource on page {url} does not specify a page name in {resourceRoot}");
}

#nullable disable