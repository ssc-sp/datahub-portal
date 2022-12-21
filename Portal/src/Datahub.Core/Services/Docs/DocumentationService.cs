using Datahub.Core.Services.Wiki;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

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
#nullable enable
public record DocItem(int Level)
{
    public string? Title { get; set; }
    public List<DocItem> Childs { get; set; } = new List<DocItem>();
    
    public string? MarkdownPage { get; set; }
}

public class DocumentationService 
{
    private readonly string _docsRoot;
    private readonly string _docsEditPrefix;
    private readonly ILogger<DocumentationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string DOCS_ROOT_CONFIG_KEY = "docsURL";
    private const string DOCS_EDIT_URL_CONFIG_KEY = "EditdocsURLPrefix";

    //TODO: use proper caching
    private MarkdownLanguageRoot? EnglishLanguageRoot = default;
    private MarkdownLanguageRoot? FrenchLanguageRoot = default;

    private IList<TimeStampedStatus> _statusMessages;

    public event Func<Task>? NotifyRefreshErrors;

    public DocumentationService(IConfiguration config, ILogger<DocumentationService> logger, IHttpClientFactory httpClientFactory)
    {
        _docsRoot = config.GetValue(DOCS_ROOT_CONFIG_KEY, "https://raw.githubusercontent.com/ssc-sp/datahub-docs/main/")!;
        _docsEditPrefix = config[DOCS_EDIT_URL_CONFIG_KEY]!;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _statusMessages = new List<TimeStampedStatus>();
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
        using var client = _httpClientFactory.CreateClient();
        try
        {
            var content = await client.GetStringAsync(fullUrl);

            var result = substitutions == null ?
                content :
                substitutions.Aggregate(content, (current, s) => current.Replace(s.Item1, s.Item2));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot load page url: {FullUrl}", fullUrl);
            return null;
        }

    }

    private static IList<LinkInline>? GetListedLinks(string inputMarkdown)
    {
        if (string.IsNullOrEmpty(inputMarkdown))
        {
            return default;
        }

        var doc = Markdown.Parse(inputMarkdown);
        
        
        var listBlock = doc.FirstOrDefault(e => e is ListBlock);
        var root = new DocItem(0);
        ProcessBlock(doc, 0, null, root);
        return listBlock?.Descendants()
            .Where(e => e is LinkInline)
            .Cast<LinkInline>()
            .ToList();
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

    private async Task<MarkdownCard?> PopulateResourceCard(LinkInline link, MarkdownCategory category)
    {
        var path = GetPath(category);
        var linkUrlMD = $"{link.Url}.md";
        var content = await LoadDocsPage(linkUrlMD);

        if (string.IsNullOrEmpty(content))
        {
            return default;
        }

        var cardDoc = Markdown.Parse(content);
        var cardDocFlattened = cardDoc.Descendants();

        var firstHeading = cardDocFlattened.FirstOrDefault(e => e is HeadingBlock) as HeadingBlock;
        var firstPara = cardDocFlattened.FirstOrDefault(e => e is ParagraphBlock) as ParagraphBlock;
        if (firstHeading?.Inline?.FirstChild is null || firstPara?.Inline?.FirstChild is null)
        {
            await AddStatusMessage($"Invalid card {link} - first Header or first Paragraph missing");
            return default;
        }

        var title = firstHeading.Inline.FirstChild.ToString();
        var preview = firstPara.Inline.FirstChild.ToString();

        var card = new MarkdownCard(title, preview, link.Url, category);

        return await Task.FromResult(card);
    }

    private async Task<MarkdownCategory?> PopulateResourceCategory(LinkInline link, MarkdownLanguageRoot languageRoot)
    {
        if (link?.FirstChild is null)
        {
            await AddStatusMessage($"Invalid card {link} - first Header or first Paragraph missing");
            return default;
        }
        var title = link.FirstChild.ToString();
        var category = new MarkdownCategory(title, languageRoot);

        var catSidebar = await LoadDocsPage(SIDEBAR);
        var catLinks = GetListedLinks(catSidebar!);

        if (catLinks?.Count > 0)
        {
            foreach (var l in catLinks)
            {
                await PopulateResourceCard(l, category);
            }
        }

        return await Task.FromResult(category);
    }

    private async Task<MarkdownLanguageRoot?> PopulateResourceLanguageRoot(LinkInline link)
    {
        if (link?.FirstChild is null)
        {
            await AddStatusMessage($"Invalid card {link} - first Header or first Paragraph missing");
            return default;
        }
        var title = link.FirstChild.ToString();
        var langRoot = new MarkdownLanguageRoot(title);

        var langSidebar = await LoadDocsPage(SIDEBAR);
        var langLinks = GetListedLinks(langSidebar!);

        if (langLinks?.Count > 0)
        {
            foreach (var l in langLinks)
            {
                await PopulateResourceCategory(l, langRoot);
            }
        }

        return await Task.FromResult(langRoot);
    }

    public async Task RefreshCache() => await LoadResourceTree();

    private async Task InvokeNotifyRefreshErrors()
    {
        if (NotifyRefreshErrors != null)
        {
            await NotifyRefreshErrors.Invoke();
        }
    }

    private async Task LoadResourceTree()
    {
        _statusMessages = new List<TimeStampedStatus>();

        await AddStatusMessage("Loading resources");

        var rootSidebar = await LoadDocsPage(SIDEBAR);
        if (rootSidebar is null)
            throw new InvalidOperationException("Sidebar is missing");
        var rootLinks = GetListedLinks(rootSidebar);
        if (rootLinks is null)
            throw new InvalidOperationException("Cannot load sidebar and content");
        var enLink = rootLinks[0];
        var frLink = rootLinks[1];

        EnglishLanguageRoot = await PopulateResourceLanguageRoot(enLink);
        FrenchLanguageRoot = await PopulateResourceLanguageRoot(frLink);

        await AddStatusMessage("Finished loading resources");

        await Task.CompletedTask;
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

    private async Task<string?> LoadDocsPage(string name)
    {
        var url = BuildUrl(name);

        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            return await httpClient.GetStringAsync(url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading {url}", url);
            await AddStatusMessage($"Error loading {url}");

            return default(string);
        }
    }

    public static bool CompareCulture(string c1, string c2)
    {
        var ci1 = CultureInfo.GetCultureInfo(c1);
        var ci2 = CultureInfo.GetCultureInfo(c2);
        return ci1.Equals(ci2) || ci1.Parent.Equals(ci2) || ci2.Parent.Equals(ci1);
    }

    public async Task<MarkdownLanguageRoot?> LoadLanguageRoot(string locale)
    {
        if (EnglishLanguageRoot == null || FrenchLanguageRoot == null)
        {
            await LoadResourceTree();
        }

        var result = CompareCulture(locale,"fr") ? FrenchLanguageRoot : EnglishLanguageRoot;
        return result;
    }

    public async Task<string?> LoadResourcePage(MarkdownCard card)
    {
        var name = $"{card.Url}.md";
        return await LoadDocsPage(name);
    }

    public string GetEditUrl(MarkdownCard card) => $"{_docsEditPrefix}{card.Url}/_edit";

    public IReadOnlyList<TimeStampedStatus> GetErrorList() => _statusMessages.AsReadOnly();

    public async Task LogNotFoundError(string pageName, string resourceRoot) => await AddStatusMessage($"{pageName} was not found in {resourceRoot} cache");

    public async Task LogNoArticleSpecifiedError(string url, string resourceRoot) => await AddStatusMessage($"Embedded resource on page {url} does not specify a page name in {resourceRoot}");
}

#nullable disable