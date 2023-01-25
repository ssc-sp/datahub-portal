using Datahub.Core.Services.Wiki;
using Datahub.Shared.Annotations;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

    public event Func<Task>? NotifyRefreshErrors;

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

    private async Task AddStatusMessage(string message)
    {
        var error = new TimeStampedStatus(DateTime.UtcNow, message);
        _statusMessages.Add(error);
        await InvokeNotifyRefreshErrors();
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
            await BuildPreview(doc);
        }
        foreach (var item in doc.Children)
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

    public const string LOCALE_EN = "";
    public const string LOCALE_FR = "fr";

    private async Task LoadResourceTree(DocumentationGuide guide, bool useCache = true)
    {
        var fileMappings = await LoadDocsPage(DocumentationGuide.RootFolder, FILE_MAPPINGS, null,useCache);
        var docFileMappings = new DocumentationFileMapper(fileMappings);

        _statusMessages = new List<TimeStampedStatus>();

        await AddStatusMessage("Loading resources");

        enOutline = ParseSidebar(guide, await LoadDocsPage(guide, SIDEBAR, LOCALE_EN, useCache), docFileMappings.GetEnglishDocumentId);
        if (enOutline is null)
            throw new InvalidOperationException("Cannot load sidebar and content");

        frOutline = ParseSidebar(guide, await LoadDocsPage(guide, $"{SIDEBAR}", LOCALE_FR, useCache), docFileMappings.GetFrenchDocumentId);
        if (frOutline is null)
            throw new InvalidOperationException("Cannot load sidebar and content");
        
        await AddStatusMessage("Finished loading sidebars");

    }

    public DocItem? LoadPage(string id, bool isFrench)
    {
        var searchRoot = isFrench ? frOutline : enOutline;
        if (searchRoot is null)
            throw new InvalidOperationException("sidebar not loaded");
        return searchRoot.LocateID(id);
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

    private string BuildUrl(DocumentationGuide guide, string? locale, string name, IList<string>? folders = null)
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
    private async Task<string?> LoadDocsPage(DocumentationGuide guide, string name, string? locale = "", bool useCache = true)
    {
        return await LoadDocs(BuildUrl(guide, locale??string.Empty, name), useCache);
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
        var url = BuildUrl(item.DocumentationGuide,null,item.GetMarkdownFileName());
        _cache.Remove(url);
    }

    public IReadOnlyList<TimeStampedStatus> GetErrorList() => _statusMessages.AsReadOnly();

    public async Task LogNotFoundError(string pageName, string resourceRoot) => await AddStatusMessage($"{pageName} was not found in {resourceRoot} cache");

    public async Task LogNoArticleSpecifiedError(string url, string resourceRoot) => await AddStatusMessage($"Embedded resource on page {url} does not specify a page name in {resourceRoot}");
}

#nullable disable