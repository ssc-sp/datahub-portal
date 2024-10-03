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
    private const string WikirootConfigKey = "WikiURL";
    private const string WikiEditUrlConfigKey = "EditWikiURLPrefix";

    private readonly string _wikiRoot;
    private readonly string _wikiEditPrefix;
    private readonly ILogger<WikiService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    //TODO: use proper caching
    private MarkdownLanguageRoot _englishLanguageRoot;
    private MarkdownLanguageRoot _frenchLanguageRoot;

    private IList<TimeStampedStatus> _errorList;

    public event Func<Task> NotifyRefreshErrors;

    public WikiService(IConfiguration config, ILogger<WikiService> logger, IHttpClientFactory httpClientFactory)
    {
        _wikiRoot = config[WikirootConfigKey];
        _wikiEditPrefix = config[WikiEditUrlConfigKey];
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _errorList = new List<TimeStampedStatus>();
    }

    private async Task AddErrorMessage(string message)
    {
        var error = new TimeStampedStatus(DateTime.UtcNow, message);
        _errorList.Add(error);
        await InvokeNotifyRefreshErrors();
    }

    public async Task<string> LoadPage(string name, List<(string Placeholder, string Substitution)> substitutions = null)
    {
        string nameTrimmed = name.TrimStart('/');

        var fullUrl = $"{_wikiRoot}{nameTrimmed}.md";
        using var client = _httpClientFactory.CreateClient();
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
            _logger.LogError(ex, "Cannot load page url: {FullUrl}", fullUrl);
            return null;
        }
    }

    private static IList<LinkInline> ListedLinks(string inputMarkdown)
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

    private IList<string> Path(AbstractMarkdownPage resource)
    {
        if (resource is null)
        {
            return new List<string>();
        }

        var parentPath = Path(resource.Parent);
        parentPath.Add(CleanupCharacters(resource.Title));
        return parentPath;
    }

    private async Task<MarkdownCard> PopulateResourceCard(LinkInline link, MarkdownCategory category)
    {
        var path = Path(category);
        var linkUrlMd = $"{link.Url}.md";
        var content = await LoadWikiPage(linkUrlMd, path);

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

        var catSidebar = await LoadSidebar(Path(category));
        var catLinks = ListedLinks(catSidebar);

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

        var langSidebar = await LoadSidebar(Path(langRoot));
        var langLinks = ListedLinks(langSidebar);

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
        _errorList = new List<TimeStampedStatus>();

        await AddErrorMessage("Loading resources");

        var rootSidebar = await LoadSidebar();
        var rootLinks = ListedLinks(rootSidebar);
        if (rootLinks != null)
        {
            var enLink = rootLinks[0];
            var frLink = rootLinks[1];

            _englishLanguageRoot = await PopulateResourceLanguageRoot(enLink);
            _frenchLanguageRoot = await PopulateResourceLanguageRoot(frLink);

            await AddErrorMessage("Finished loading resources");
        }
        else
        {
            _logger.LogWarning($"No data found for root sidebar {rootSidebar}");
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

        sb.Append(_wikiRoot);
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

        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            var result = await httpClient.GetStringAsync(url);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading {url}", url);
            await AddErrorMessage($"Error loading {url}");

            return await Task.FromResult(default(string));
        }
    }

    public async Task<MarkdownLanguageRoot> LoadLanguageRoot(bool isFrench)
    {
        if (_englishLanguageRoot == null || _frenchLanguageRoot == null)
        {
            await LoadResourceTree();
        }

        var result = isFrench ? _frenchLanguageRoot : _englishLanguageRoot;
        return await Task.FromResult(result);
    }

    public async Task<string> LoadResourcePage(MarkdownCard card)
    {
        var path = Path(card.ParentCategory);
        var name = $"{card.Url}.md";
        return await LoadWikiPage(name, path);
    }

    public string GetEditUrl(MarkdownCard card) => $"{_wikiEditPrefix}{card.Url}/_edit";

    public IReadOnlyList<TimeStampedStatus> GetErrorList() => _errorList.AsReadOnly();

    public async Task LogNotFoundError(string pageName, string resourceRoot) => await AddErrorMessage($"{pageName} was not found in {resourceRoot} cache");

    public async Task LogNoArticleSpecifiedError(string url, string resourceRoot) => await AddErrorMessage($"Embedded resource on page {url} does not specify a page name in {resourceRoot}");
}
