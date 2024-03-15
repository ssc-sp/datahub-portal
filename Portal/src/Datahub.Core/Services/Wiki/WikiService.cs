using System.Globalization;
using System.Text;
using Datahub.Markdown.Model;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Datahub.Core.Services.Wiki;

public class WikiService : IWikiService
{
    private const string WIKIROOT_CONFIG_KEY = "WikiURL";
    private const string WIKI_EDIT_URL_CONFIG_KEY = "EditWikiURLPrefix";

    private readonly string wikiRoot;
    private readonly string wikiEditPrefix;
    private readonly ILogger<WikiService> logger;
    private readonly IHttpClientFactory httpClientFactory;

    //TODO: use proper caching
    private MarkdownLanguageRoot englishLanguageRoot;
    private MarkdownLanguageRoot frenchLanguageRoot;

    private IList<TimeStampedStatus> errorList;

    public event Func<Task> NotifyRefreshErrors;

    public WikiService(IConfiguration config, ILogger<WikiService> logger, IHttpClientFactory httpClientFactory)
    {
        wikiRoot = config[WIKIROOT_CONFIG_KEY];
        wikiEditPrefix = config[WIKI_EDIT_URL_CONFIG_KEY];
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
        errorList = new List<TimeStampedStatus>();
    }

    private async Task AddErrorMessage(string message)
    {
        var error = new TimeStampedStatus(DateTime.UtcNow, message);
        errorList.Add(error);
        await InvokeNotifyRefreshErrors();
    }

    public async Task<string> LoadPage(string name, List<(string Placeholder, string Substitution)> substitutions = null)
    {
        string nameTrimmed = name.TrimStart('/');

        var fullUrl = $"{wikiRoot}{nameTrimmed}.md";
        using var client = httpClientFactory.CreateClient();
        try
        {
            var content = await client.GetStringAsync(fullUrl);

            var result = substitutions == null ?
                content :
                substitutions.Aggregate(content, (current, s) => current.Replace(s.Placeholder, s.Substitution));

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot load page url: {FullUrl}", fullUrl);
            return null;
        }
    }

    private static IList<LinkInline> GetListedLinks(string inputMarkdown)
    {
        if (string.IsNullOrEmpty(inputMarkdown))
        {
            return default;
        }

        var doc = Markdig.Markdown.Parse(inputMarkdown);
        var listBlock = doc.FirstOrDefault(e => e is ListBlock);
        return listBlock?.Descendants()
            .Where(e => e is LinkInline)
            .Cast<LinkInline>()
            .ToList();
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

    private async Task<MarkdownCard> PopulateResourceCard(LinkInline link, MarkdownCategory category)
    {
        var path = GetPath(category);
        var linkUrlMD = $"{link.Url}.md";
        var content = await LoadWikiPage(linkUrlMD, path);

        if (string.IsNullOrEmpty(content))
        {
            return await Task.FromResult(default(MarkdownCard));
        }

        var cardDoc = Markdig.Markdown.Parse(content);
        var cardDocFlattened = cardDoc.Descendants();

        var firstHeading = cardDocFlattened.FirstOrDefault(e => e is HeadingBlock) as HeadingBlock;
        var firstPara = cardDocFlattened.FirstOrDefault(e => e is ParagraphBlock) as ParagraphBlock;

        var title = firstHeading.Inline.FirstChild.ToString();
        var preview = firstPara.Inline.FirstChild.ToString();

        var card = new MarkdownCard(title, preview, link.Url, category);

        return await Task.FromResult(card);
    }

    private async Task<MarkdownCategory> PopulateResourceCategory(LinkInline link, MarkdownLanguageRoot languageRoot)
    {
        var title = link.FirstChild.ToString();
        var category = new MarkdownCategory(title, languageRoot);

        var catSidebar = await LoadSidebar(GetPath(category));
        var catLinks = GetListedLinks(catSidebar);

        if (catLinks?.Count > 0)
        {
            foreach (var l in catLinks)
            {
                await PopulateResourceCard(l, category);
            }
        }

        return await Task.FromResult(category);
    }

    private async Task<MarkdownLanguageRoot> PopulateResourceLanguageRoot(LinkInline link)
    {
        var title = link.FirstChild.ToString();
        var langRoot = new MarkdownLanguageRoot(title);

        var langSidebar = await LoadSidebar(GetPath(langRoot));
        var langLinks = GetListedLinks(langSidebar);

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
        errorList = new List<TimeStampedStatus>();

        await AddErrorMessage("Loading resources");

        var rootSidebar = await LoadSidebar();
        var rootLinks = GetListedLinks(rootSidebar);
        if (rootLinks != null)
        {
            var enLink = rootLinks[0];
            var frLink = rootLinks[1];

            englishLanguageRoot = await PopulateResourceLanguageRoot(enLink);
            frenchLanguageRoot = await PopulateResourceLanguageRoot(frLink);

            await AddErrorMessage("Finished loading resources");
        }
        else
        {
            logger.LogWarning($"No data found for root sidebar {rootSidebar}");
        }
    }

    private static string BuildSidebarName(IList<string> folders = null)
    {
        StringBuilder sb = new();

        if (folders?.Count > 0)
        {
            foreach (var f in folders)
            {
                sb.Append($"_{f}");
            }
        }

        sb.Append("_Sidebar.md");
        return sb.ToString();
    }

    private string BuildUrl(string name, IList<string> folders = null)
    {
        StringBuilder sb = new();

        sb.Append(wikiRoot);
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

    private async Task<string> LoadSidebar(IList<string> folders = null)
    {
        var sbName = BuildSidebarName(folders);
        return await LoadWikiPage(sbName, folders);
    }

    private async Task<string> LoadWikiPage(string name, IList<string> folders = null)
    {
        var url = BuildUrl(name, folders);

        var httpClient = httpClientFactory.CreateClient();
        try
        {
            var result = await httpClient.GetStringAsync(url);
            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error loading {url}", url);
            await AddErrorMessage($"Error loading {url}");

            return await Task.FromResult(default(string));
        }
    }

    public async Task<MarkdownLanguageRoot> LoadLanguageRoot(bool isFrench)
    {
        if (englishLanguageRoot == null || frenchLanguageRoot == null)
        {
            await LoadResourceTree();
        }

        var result = isFrench ? frenchLanguageRoot : englishLanguageRoot;
        return await Task.FromResult(result);
    }

    public async Task<string> LoadResourcePage(MarkdownCard card)
    {
        var path = GetPath(card.ParentCategory);
        var name = $"{card.Url}.md";
        return await LoadWikiPage(name, path);
    }

    public string GetEditUrl(MarkdownCard card) => $"{wikiEditPrefix}{card.Url}/_edit";

    public IReadOnlyList<TimeStampedStatus> GetErrorList() => errorList.AsReadOnly();

    public async Task LogNotFoundError(string pageName, string resourceRoot) => await AddErrorMessage($"{pageName} was not found in {resourceRoot} cache");

    public async Task LogNoArticleSpecifiedError(string url, string resourceRoot) => await AddErrorMessage($"Embedded resource on page {url} does not specify a page name in {resourceRoot}");
}
