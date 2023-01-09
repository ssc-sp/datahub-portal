using Datahub.Core.Services.Wiki;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services.Docs;
#nullable enable
public record DocItem(int Level, bool isRoot = false)
{
    public string? Title { get; set; }
    public List<DocItem> Childs { get; set; } = new List<DocItem>();
    
    public string? Preview { get; set; }

    public string? ContentTitle { get; set; }

    public string? Content { get; set; }

    public string? MarkdownPage { get; set; }

    public string GetDescription() => $"Card '{Title}' - '{MarkdownPage}'";    

    public string GetID()
    {
        if (isRoot) return DocumentationService.GetPageCode("root");
        if (Title is null && MarkdownPage is null) throw new InvalidDataException($"Page has no Title and no URL");
        return DocumentationService.GetPageCode(MarkdownPage ?? Title!);
    }

    public DocItem? LocateID(string id)
    {
        if (string.Equals(id, GetID(), StringComparison.InvariantCultureIgnoreCase)) return this;
        if (Childs is null || Childs.Count == 0)
            return null;
        foreach (var item in Childs)
        {
            var found = item.LocateID(id);
            if (found != null)
                return found;
        }
        return null;

    }
}

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

    public event Func<Task>? NotifyRefreshErrors;

    public DocumentationService(IConfiguration config, ILogger<DocumentationService> logger, 
        IHttpClientFactory httpClientFactory,
        IMemoryCache docCache)
    {
        _docsRoot = config.GetValue(DOCS_ROOT_CONFIG_KEY, "https://raw.githubusercontent.com/ssc-sp/datahub-docs/main/")!;
        _docsEditPrefix = config[DOCS_EDIT_URL_CONFIG_KEY]!;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _statusMessages = new List<TimeStampedStatus>();
        _cache = docCache;
    }

    private async Task AddStatusMessage(string message)
    {
        var error = new TimeStampedStatus(DateTime.UtcNow, message);
        _statusMessages.Add(error);
        await InvokeNotifyRefreshErrors();
    }

    public async Task<string?> LoadPage(string name, List<(string, string)>? substitutions = null)
    {
        string nameTrimmed = name.TrimStart('/');


        var fullUrl = $"{_docsRoot}{nameTrimmed}.md";
        try
        {
            var content = await LoadDocsPage(fullUrl);

            var result = substitutions == null ?
                content :
                substitutions.Aggregate(content, (current, s) => current?.Replace(s.Item1, s.Item2));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot load page url: {FullUrl}", fullUrl);
            return null;
        }

    }

    private static DocItem? ParseSidebar(string? inputMarkdown)
    {
        if (string.IsNullOrEmpty(inputMarkdown))
        {
            return default;
        }

        var doc = Markdown.Parse(inputMarkdown);
        
        var root = new DocItem(0,true);
        ProcessBlock(doc, 0, null, root);
        return root;
    }

    private static DocItem? ProcessBlock(MarkdownObject markdownObject, int level, DocItem? currentItem,DocItem? parent)
    {
        switch (markdownObject)
        {
            case LiteralInline literalInline:
                if (currentItem is null)
                {
                    var docItem1 = new DocItem(level) { Title = literalInline.ToString() };
                    parent?.Childs.Add(docItem1);
                    return docItem1;
                }
                else
                {
                    currentItem.Title = literalInline.ToString();
                }
                return null;
            case LinkInline linkInline:
                //[Microservice_Architecture](/Architecture/Microservice_Architecture.md)
                var docItem = new DocItem(level) { Title = linkInline.Title};
                parent?.Childs.Add(docItem);
                docItem.MarkdownPage = linkInline.Url;
                foreach (var child in linkInline)
                {
                    ProcessBlock(child, level, docItem, parent);
                }
                return docItem;
            case LeafBlock paragraphBlock:
                if (paragraphBlock.Inline != null)
                    return ProcessBlock(paragraphBlock.Inline, level, currentItem,parent);
                break;

            case ContainerInline inline:
                DocItem? newDoc = null;
                foreach (var child in inline)
                {
                    var res = ProcessBlock(child, level, currentItem,parent);
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
                    var res2 = ProcessBlock(child, currentLevel, currentItem, (containerBlock is ListItemBlock)?currentParent: parent);
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
        if (doc.Title != null)
        {
            doc.Content = await LoadDocsPage(doc.Title);
            await BuildPreview(doc);
        }
        foreach (var item in doc.Childs)
        {
            await BuildDocAndPreviews(item);
        }
    }

    private async Task BuildPreview(DocItem doc)
    {
        if (string.IsNullOrEmpty(doc.Content)) return;
        var cardDoc = Markdown.Parse(doc.Content);
        var cardDocFlattened = cardDoc.Descendants();

        var firstHeading = cardDocFlattened.FirstOrDefault(e => e is HeadingBlock) as HeadingBlock;
        var firstPara = cardDocFlattened.FirstOrDefault(e => e is ParagraphBlock) as ParagraphBlock;
        if (firstHeading?.Inline?.FirstChild is null || firstPara?.Inline?.FirstChild is null)
        {
            await AddStatusMessage($"Invalid card {doc.GetDescription()} - first Header or first Paragraph missing");
            return;
        }

        var title = firstHeading.Inline.FirstChild.ToString();
        var preview = firstPara.Inline.FirstChild.ToString();

        doc.ContentTitle = title;
        doc.Preview = preview;
    }

    private async Task InvokeNotifyRefreshErrors()
    {
        if (NotifyRefreshErrors != null)
        {
            await NotifyRefreshErrors.Invoke();
        }
    }

    private async Task LoadResourceTree(bool useCache = true)
    {
        _statusMessages = new List<TimeStampedStatus>();

        await AddStatusMessage("Loading resources");

        enOutline = ParseSidebar(await LoadDocsPage(SIDEBAR, useCache));
        if (enOutline is null)
            throw new InvalidOperationException("Cannot load sidebar and content");
        frOutline = ParseSidebar(await LoadDocsPage($"fr/{SIDEBAR}", useCache));
        if (frOutline is null)
            throw new InvalidOperationException("Cannot load sidebar and content");
        
        await AddStatusMessage("Finished loading sidebars");

    }

    public DocItem? LoadPage(string id)
    {
        if (enOutline is null)
            throw new InvalidOperationException("sidebar not loaded");
        var enResult = enOutline.LocateID(id);
        if (enResult != null)
            return enResult;
        if (frOutline is null)
            throw new InvalidOperationException("sidebar not loaded");
        return frOutline.LocateID(id);
    }

    public DocItem? GetParent(DocItem docItem, DocItem? currentNode = null)
    {
        if (docItem == enOutline || docItem == frOutline)
            return null;
        if (currentNode is null)
        {
            return GetParent(docItem, enOutline) ?? GetParent(docItem, frOutline);
        }
        if (currentNode.Childs is null || currentNode.Childs.Count == 0)
            return null;
        foreach (var item in currentNode.Childs)
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

    private string BuildUrl(string name, IList<string>? folders = null)
    {
        StringBuilder sb = new();

        sb.Append(_docsRoot);
        if (folders?.Count > 0)
        {
            foreach (var f in folders)
            {
                sb.Append($"{f}/");
            }
        }
        sb.Append(name);

        return sb.ToString();
    }


    private async Task<string?> LoadDocsPage(string name, bool useCache = true)
    {
        return await LoadDocs(BuildUrl(name), useCache);
    }

    private async Task<string?> LoadDocs(string url, bool useCache = true)
    {
        if (_cache.TryGetValue(url, out var docContent) && useCache)
            return docContent as string;
        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            var content = await httpClient.GetStringAsync(url);
            var key = _cache.CreateEntry(url);
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
            await AddStatusMessage($"Error loading {url}");

            return default(string);
        }
    }
    
    public static long GetStableHashCode(string str)
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

    public async Task<DocItem?> GetLanguageRoot(string locale, bool useCache = true)
    {
        if (enOutline == null || frOutline == null)
        {
            await LoadResourceTree(useCache);
        }

        var result = CompareCulture(locale,"fr") ? frOutline : enOutline;
        return result;
    }

    public async Task<string?> LoadResourcePage(DocItem card)
    {
        var name = $"{card.Title}.md";
        return await LoadDocsPage(name);
    }

    public string GetEditUrl(DocItem card) => $"{_docsEditPrefix}{card.Title}/_edit";

    public IReadOnlyList<TimeStampedStatus> GetErrorList() => _statusMessages.AsReadOnly();

    public async Task LogNotFoundError(string pageName, string resourceRoot) => await AddStatusMessage($"{pageName} was not found in {resourceRoot} cache");

    public async Task LogNoArticleSpecifiedError(string url, string resourceRoot) => await AddStatusMessage($"Embedded resource on page {url} does not specify a page name in {resourceRoot}");
}

#nullable disable