using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using Datahub.Core.Services.Wiki;
using Datahub.Markdown;
using Datahub.Markdown.Model;
using Datahub.Shared.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace Datahub.Core.Services.Docs;

#nullable enable

public class DocumentationService
{
    private const string DocsRootConfigKey = "docsURL";
    private const string DocsEditUrlConfigKey = "EditdocsURLPrefix";

    public const string LocaleEn = "";
    public const string LocaleFr = "fr";
    public const string Sidebar = "_sidebar.md";
    public const string FileMappings = "filemappings.json";
    private const string LastCommitTs = "LAST_COMMIT_TS";
    public const string CommitApiUrl = "https://api.github.com/repos/ssc-sp/datahub-docs/commits";
    private const string ContainerName = "docs";

    private readonly string _docsEditPrefix;
    private readonly ILogger<DocumentationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    private DocumentationFileMapper _docFileMappings = null!;
    private IList<TimeStampedStatus> _statusMessages;
    private DocItem? _enOutline;
    private DocItem? _frOutline;
    private DocItem _cachedDocs;
    private readonly IMemoryCache _cache;

    public DocumentationService(IConfiguration config, ILogger<DocumentationService> logger,
        IHttpClientFactory httpClientFactory, IWebHostEnvironment environment,
        IMemoryCache docCache)
    {
        //!ctx.HostingEnvironment.IsDevelopment()

        var branch = environment.IsProduction() ? "main" : "next";
        _docsEditPrefix = config.GetValue(DocsEditUrlConfigKey, $"https://github.com/ssc-sp/datahub-docs/edit/{branch}/")!;
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _statusMessages = new List<TimeStampedStatus>();
        _cache = docCache;
        _cachedDocs = DocItem.MakeRoot(DocumentationGuideRootSection.Hidden, "Cached");
    }

    /// <summary>
    /// Invalidates the cache by removing all entries from the memory cache and reloading the resource tree.
    /// </summary>
    /// <returns>A boolean value indicating whether the cache was successfully invalidated.</returns>
    public async Task<bool> InvalidateCache()
    {
        try
        {
            var cache = _cache as MemoryCache;
            if (cache != null)
            {
                // Clear all entries from the memory cache
                var percentage = 1.0; // 100%
                cache.Compact(percentage);

                // Reload the resource tree
                await LoadResourceTree(DocumentationGuideRootSection.UserGuide);

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

    /// <summary>
    /// Adds a status message to the list of time-stamped status messages.
    /// </summary>
    /// <param name="message">The message to add.</param>
    private void AddStatusMessage(string message)
    {
        var error = new TimeStampedStatus(DateTime.UtcNow, message);
        _statusMessages.Add(error);
    }

    /// <summary>
    /// Builds the absolute URL for a relative link by combining it with the blob root URL. Used for images within markdown.
    /// </summary>
    /// <param name="relLink">The relative link.</param>
    /// <returns>The absolute URL.</returns>
    public string BuildAbsoluteUrl(string relLink)
    {
        if (relLink is null)
        {
            throw new ArgumentNullException(nameof(relLink));
        }

        return new Uri($"https://raw.githubusercontent.com/ssc-sp/datahub-docs/next/{relLink}").AbsoluteUri;
    }

    /// <summary>
    /// Cleans up the characters in the input string by normalizing and replacing spaces with hyphens.
    /// </summary>
    /// <param name="input">The input string to be cleaned up.</param>
    /// <returns>The cleaned up string.</returns>
    private string CleanupCharacters(string input)
    {
        var deAccented = new string(input?.Normalize(NormalizationForm.FormD)
            .ToCharArray()
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());

        var deSpaced = deAccented.Replace(" ", "-");

        return deSpaced;
    }

    /// <summary>
    /// Retrieves the path of a given resource by traversing up the resource hierarchy and appending the cleaned-up titles of each parent resource.
    /// </summary>
    /// <param name="resource">The resource for which to retrieve the path.</param>
    /// <returns>A list of strings representing the path of the resource.</returns>
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

    /// <summary>
    /// Builds the documentation content and previews for a given DocItem.
    /// </summary>
    /// <param name="doc">The DocItem to build the content and previews for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task BuildDocAndPreviews(DocItem doc)
    {
        if (doc.DocType == DocItemType.External)
        {
            doc.Content = null;
            doc.Preview = "External Web Link";
        }
        else
        {
            if (doc.Title is not null)
            {
                doc.Content = await LoadDocsPage(DocumentationGuideRootSection.RootFolder, doc.GetMarkdownFileName());
                BuildPreview(doc);
            }
            else
            {
                doc.Content = null;
                doc.Preview = String.Join(" ,", doc.Children.Select(d => d.Title));
            }
        }
        foreach (var item in doc.Children.ToList())
        {
            await BuildDocAndPreviews(item);
        }
    }

    /// <summary>
    /// Builds the preview content for a given DocItem.
    /// </summary>
    /// <param name="doc">The DocItem to build the preview content for.</param>
    private void BuildPreview(DocItem doc)
    {
        if (string.IsNullOrEmpty(doc.Content))
        {
            doc.Preview = String.Join(", ", doc.Children.Select(d => d.Title));
            return;
        }

        var cardContent = MarkdownTools.GetTitleAndPreview(doc.Content);
        if (cardContent is null)
        {
            doc.ContentTitle = null;
            doc.Preview = String.Join(" ,", doc.Children.Select(d => d.Title));
            AddStatusMessage($"Invalid card {doc.GetDescription()} - first Header or first Paragraph missing");
        }
        else
        {
            doc.ContentTitle = cardContent.Value.Title;
            doc.Preview = cardContent.Value.Preview;
        }
    }

    /// <summary>
    /// Loads the resource tree for the given documentation guide.
    /// </summary>
    /// <param name="guide">The documentation guide to load the resource tree for.</param>
    /// <param name="useCache">A boolean value indicating whether to use the cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadResourceTree(DocumentationGuideRootSection guide, bool useCache = true)
    {
        var fileMappings = await LoadDocsPage(DocumentationGuideRootSection.RootFolder, FileMappings, null, useCache);
        _docFileMappings = new DocumentationFileMapper(fileMappings);

        _statusMessages = new List<TimeStampedStatus>();

        AddStatusMessage("Loading resources");

        _enOutline = SidebarParser.ParseSidebar(guide, await LoadDocsPage(guide, Sidebar, LocaleEn, useCache), _docFileMappings.GetEnglishDocumentId);
        if (_enOutline is null)
            throw new InvalidOperationException($"Cannot load sidebar and content");

        _frOutline = SidebarParser.ParseSidebar(guide, await LoadDocsPage(guide, Sidebar, LocaleFr, useCache), _docFileMappings.GetFrenchDocumentId);
        if (_frOutline is null)
            throw new InvalidOperationException("Cannot load sidebar and content");
        _cachedDocs = DocItem.MakeRoot(DocumentationGuideRootSection.Hidden, "Cached");
        AddStatusMessage("Finished loading sidebars");
    }

    /// <summary>
    /// Loads the page with the specified ID from the resource tree.
    /// </summary>
    /// <param name="id">The ID of the page to load.</param>
    /// <param name="isFrench">A boolean value indicating whether the page is in French.</param>
    /// <returns>The loaded DocItem if found, otherwise null.</returns>
    public DocItem? LoadPage(string id, bool isFrench)
    {
        var searchRoot = isFrench ? _frOutline : _enOutline;
        if (searchRoot is null)
            throw new InvalidOperationException("sidebar not loaded");
        return searchRoot.LocateID(id);
    }

    /// <summary>
    /// Loads the page with the specified path from the resource tree.
    /// </summary>
    /// <param name="path">The path of the page to load.</param>
    /// <param name="isFrench">A boolean value indicating whether the page is in French.</param>
    /// <returns>The loaded DocItem if found, otherwise null.</returns>
    public async Task<DocItem?> LoadPageFromPath(string path, bool isFrench)
    {
        var searchRoot = isFrench ? _frOutline : _enOutline;
        if (searchRoot is null)
            throw new InvalidOperationException("sidebar not loaded");
        var inCachePage = searchRoot.LocatePath(path);
        if (inCachePage is null)
        {
            inCachePage = _cachedDocs.LocatePath(path);
            if (inCachePage != null)
                return inCachePage;
            var itemId = (isFrench ? _docFileMappings?.GetFrenchDocumentId(path) : _docFileMappings?.GetEnglishDocumentId(path)) ?? MarkdownTools.GetIDFromString(path);
            var docItem = DocItem.GetItem(DocumentationGuideRootSection.Hidden, itemId, searchRoot.Level + 1, path, path);

            _cachedDocs.Children.Add(docItem);
            await BuildDocAndPreviews(docItem);
            return docItem;
        }
        return inCachePage;
    }

    /// <summary>
    /// Retrieves the parent of a given DocItem in the resource tree.
    /// </summary>
    /// <param name="docItem">The DocItem for which to retrieve the parent.</param>
    /// <param name="currentNode">The current node being traversed in the resource tree.</param>
    /// <returns>The parent DocItem if found, otherwise null.</returns>
    public DocItem? Parent(DocItem docItem, DocItem? currentNode = null)
    {
        if (docItem == _enOutline || docItem == _frOutline)
            return null;
        if (currentNode is null)
        {
            return Parent(docItem, _enOutline) ?? Parent(docItem, _frOutline);
        }
        if (currentNode.Children is null || currentNode.Children.Count == 0)
            return null;
        foreach (var item in currentNode.Children)
        {
            if (item == docItem)
                return currentNode;
            var nextLevel = Parent(docItem, item);
            if (nextLevel != null)
                return nextLevel;
        }
        return null;
    }

    /// <summary>
    /// Builds the path for a documentation resource based on the guide, locale, name, and optional folders.
    /// </summary>
    /// <param name="guide">The documentation guide.</param>
    /// <param name="locale">The locale of the resource.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="folders">Optional folders within the resource.</param>
    /// <returns>The built path.</returns>
    private string BuildPath(DocumentationGuideRootSection guide, string? locale, string name, IList<string>? folders = null)
    {
        var allFolders = new List<string>();
        if (!string.IsNullOrEmpty(locale)) allFolders.Add(locale);
        if (!string.IsNullOrEmpty(guide.GetStringValue())) allFolders.Add(guide.GetStringValue());
        if (folders != null) allFolders.AddRange(folders);

        StringBuilder sb = new();

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
    /// Loads the documentation page from the given guide and name.
    /// </summary>
    /// <param name="guide"></param>
    /// <param name="name"></param>
    /// <param name="locale">Leave empty for "en", "fr" has its own folder</param>
    /// <param name="useCache"></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task<string?> LoadDocsPage(DocumentationGuideRootSection guide, string? name, string? locale = "", bool useCache = true)
    {
        if (name is null) return null;
        return await LoadDocs(BuildPath(guide, locale ?? string.Empty, name));
    }

    /// <summary>
    /// Retrieves the last commit timestamp for the repository.
    /// </summary>
    /// <param name="useCache">A boolean value indicating whether to use the cache.</param>
    /// <returns>The last commit timestamp if available, otherwise null.</returns>
    public async Task<DateTime?> LastRepoCommitTs(bool useCache = true)
    {
        // Check if the last commit timestamp is already in the cache
        if (_cache.TryGetValue(LastCommitTs, out DateTime? lastTs) && useCache)
        {
            if (lastTs.HasValue)
            {
                return lastTs.Value;
            }
        }

        // Read the commit information from the API
        var node = await ReadUrl(new Dictionary<string, string>() { { "path", "UserGuide/_sidebar.md" }, { "sha", "main" }, });
        var lastCommit = (DateTime?)node?[0]?["commit"]?["author"]?["date"];

        if (lastCommit.HasValue)
        {
            // Set the cache entry options
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                // Keep in cache for this time, reset time if accessed.
                .SetAbsoluteExpiration(DateTime.Now.AddHours(1));

            // Save data in cache.
            _cache.Set(LastCommitTs, lastCommit.Value, cacheEntryOptions);
            return lastCommit.Value;
        }
        _logger.LogWarning($"Cannot load last commit timestamp for user docs");
        return null;
    }

    /// <summary>
    /// Retrieves the retry policy for handling transient HTTP errors and not found status codes.
    /// </summary>
    /// <returns>The retry policy.</returns>
    private static IAsyncPolicy<HttpResponseMessage> RetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(
                2,
                retryAttempt)));
    }

    /// <summary>
    /// Reads the URL with the specified parameters and returns the JSON response.
    /// </summary>
    /// <param name="parameters">The optional parameters to include in the URL.</param>
    /// <returns>The JSON response as a <see cref="JsonNode"/> object.</returns>
    public async Task<JsonNode?> ReadUrl(Dictionary<string, string>? parameters = null)
    {
        var client = _httpClientFactory.CreateClient();

        var builder = new UriBuilder(new Uri(CommitApiUrl));
        if (parameters != null)
            builder.Query = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

        var res = await RetryPolicy().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage() { RequestUri = builder.Uri, Method = HttpMethod.Get };
            client.DefaultRequestHeaders.Add("User-Agent", "DataHub");
            return await client.SendAsync(request);
        });

        if (res.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Received status code {res.StatusCode}");
        }

        return JsonNode.Parse(await res.Content.ReadAsStreamAsync());
    }

    /// <summary>
    /// Loads the documentation page with a specified path from the standard blob storage.
    /// </summary>
    /// <param name="path">The path of the page to load.</param>
    /// <returns>The loaded documentation page if found, otherwise null.</returns>
    private async Task<string?> LoadDocs(string path)
    {
        try
        {
            var connectionString = _config["Media:StorageConnectionString"];
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            return await LoadDocs(path, blobServiceClient);
        }
        catch (Exception e)
        {
            AddStatusMessage($"Error loading {path}: {e.Message}");
            return e.Message;
        }
    }

    /// <summary>
    /// Loads the documentation page with the specified path from the given blob storage.
    /// </summary>
    /// <param name="path">The path of the page to load.</param>
    /// <param name="blobServiceClient">The blob service client to use for loading the documentation page.</param>
    /// <returns>The loaded documentation page content if found, otherwise null.</returns>
    private async Task<string?> LoadDocs(string path, BlobServiceClient blobServiceClient)
    {
        try
        {
            var storageContainerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
            var documentBlobClient = storageContainerClient.GetBlobClient(path);

            if (await documentBlobClient.ExistsAsync())
            {
                var documentResponse = await documentBlobClient.DownloadContentAsync();
                var documentContent = documentResponse.Value.Content.ToString();
                documentContent = MarkdownHelper.RemoveFrontMatter(documentContent);
                return documentContent;
            }
            else
            {
                return null;
            }
        }
        catch (Exception e)
        {
            AddStatusMessage($"Error loading {path}: {e.Message}");
            return e.Message;
        }
    }

    /// <summary>
    /// Loads the resource tree for the given documentation guide and locale.
    /// </summary>
    /// <param name="guide">The documentation guide to load the resource tree for.</param>
    /// <param name="locale">The locale of the resource tree.</param>
    /// <param name="useCache">A boolean value indicating whether to use the cache.</param>
    /// <returns>The loaded resource tree if successful, otherwise null.</returns>
    public async Task<DocItem?> LoadResourceTree(DocumentationGuideRootSection guide, string locale, bool useCache = true)
    {
        if (_enOutline == null || _frOutline == null)
        {
            await LoadResourceTree(guide, useCache);
        }

        var result = MarkdownTools.CompareCulture(locale, "fr") ? _frOutline : _enOutline;
        return result;
    }

    /// <summary>
    /// Loads the resource page for the given DocItem.
    /// </summary>
    /// <param name="card">The DocItem representing the resource page.</param>
    /// <returns>The loaded resource page if found, otherwise null.</returns>
    public async Task<string?> LoadResourcePage(DocItem card)
    {
        return await LoadDocsPage(DocumentationGuideRootSection.RootFolder, card.GetMarkdownFileName());
    }

    /// <summary>
    /// Retrieves the edit URL for the given DocItem.
    /// </summary>
    /// <param name="card">The DocItem representing the resource page.</param>
    /// <returns>The edit URL for the resource page.</returns>
    public string EditUrl(DocItem card) => $"{_docsEditPrefix}{card.GetMarkdownFileName()}";

    /// <summary>
    /// Removes the specified DocItem from the cache.
    /// </summary>
    /// <param name="item">The DocItem to remove from the cache.</param>
    public void RemoveFromCache(DocItem item)
    {
        if (item.GetMarkdownFileName != null)
        {
            var path = BuildPath(item.RootSection, null, item.GetMarkdownFileName()!);
            _cache.Remove(path);
        }
    }

    /// <summary>
    /// Retrieves the list of error messages.
    /// </summary>
    /// <returns>The list of error messages.</returns>
    public IReadOnlyList<TimeStampedStatus> ErrorList() => _statusMessages.AsReadOnly();

    /// <summary>
    /// Logs a not found error for the specified page name and resource root.
    /// </summary>
    /// <param name="pageName">The name of the page.</param>
    /// <param name="resourceRoot">The resource root.</param>
    public void LogNotFoundError(string pageName, string resourceRoot) => AddStatusMessage($"{pageName} was not found in {resourceRoot} cache");

    /// <summary>
    /// Logs a no article specified error for the specified URL and resource root.
    /// </summary>
    /// <param name="url">The URL of the page.</param>
    /// <param name="resourceRoot">The resource root.</param>
    public void LogNoArticleSpecifiedError(string url, string resourceRoot) => AddStatusMessage($"Embedded resource on page {url} does not specify a page name in {resourceRoot}");
}

#nullable disable