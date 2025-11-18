using System.Globalization;
using System.Text;
using Datahub.Markdown;
using Datahub.Markdown.Model;
using Datahub.Shared.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace Datahub.Core.Services.Docs;

#nullable enable

public class DocumentationService
{
    public const string LocaleEn = "";
    public const string LocaleFr = "fr";
    public const string Sidebar = "_sidebar.md";
    public const string FileMappings = "filemappings.json";
    private const string ContainerName = "docs";

    private readonly ILogger<DocumentationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    private DocumentationFileMapper _docFileMappings = null!;
    private IList<TimeStampedStatus> _statusMessages;
    private BlobServiceClient? _blobServiceClient;
    private DocItem? _enOutline;
    private DocItem? _frOutline;
    private DocItem _cachedDocs;
    private readonly IMemoryCache _cache;

    public DocumentationService(IConfiguration config, ILogger<DocumentationService> logger,
        IHttpClientFactory httpClientFactory, IWebHostEnvironment environment,
        IMemoryCache docCache)
    {
        //!ctx.HostingEnvironment.IsDevelopment()
        _config = config;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _statusMessages = new List<TimeStampedStatus>();
        _cache = docCache;
        _cachedDocs = DocItem.MakeRoot(DocumentationGuideRootSection.Hidden, "Cached");
        var connectionString = _config["Media:StorageConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("Storage connection string is not set in the configuration.");
            _blobServiceClient = null;
        }
        else
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }
    }

    /// <summary>
    /// Overwrite BlobServiceClient.
    /// </summary>
    /// <param name="blobServiceClient"></param>
    internal void InitBlobClient(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
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
                if (doc.Content is null)
                {
                    var name = doc.GetMarkdownFileName() ?? string.Empty;
                    var path = BuildPath(DocumentationGuideRootSection.RootFolder, string.Empty, name);
                    if (path != string.Empty)
                    {
                        doc.Content = await LoadDocsFromAzure($"{path}");
                        BuildPreview(doc);
                    }
                }
            }
            else
            {
                doc.Content = null;
                doc.Preview = string.Join(" ,", doc.Children.Select(d => d.Title));
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
        _statusMessages = new List<TimeStampedStatus>();
        AddStatusMessage("Loading resources");

        // Load file mappings from Azure
        var fileMappings = await LoadDocsFromAzure($"{FileMappings}", useCache);
        _docFileMappings = new DocumentationFileMapper(fileMappings);

        // Load sidebars from Azure
        _enOutline = SidebarParser.ParseSidebar(guide, await LoadDocsFromAzure($"{guide.GetStringValue()}/{Sidebar}", useCache), _docFileMappings.GetEnglishDocumentId);
        if (_enOutline is null) throw new InvalidOperationException("Cannot load sidebar and content");

        _frOutline = SidebarParser.ParseSidebar(guide, await LoadDocsFromAzure($"fr/{guide.GetStringValue()}/{Sidebar}", useCache), _docFileMappings.GetFrenchDocumentId);
        if (_frOutline is null) throw new InvalidOperationException("Cannot load sidebar and content");

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
        var locId = $"{id}-{(isFrench ? "FR" : "EN")}";
        if (_cache.TryGetValue(locId, out DocItem? cachedPage))
        {
            return cachedPage;
        }
        var searchRoot = isFrench ? _frOutline : _enOutline;
        if (searchRoot is null)
            throw new InvalidOperationException("sidebar not loaded");
        var docItem = searchRoot.LocateID(id);
        _cache.Set(locId, docItem, GetEntryOptions());
        return docItem;
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
        if (!string.IsNullOrEmpty(guide.GetStringValue())) allFolders.Add(guide.GetStringValue()!);
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

        string documentPath = BuildPath(guide, locale ?? string.Empty, name);

        // Fetch from Azure Blob Storage
        return await LoadDocsFromAzure(documentPath);
    }

    /// <summary>
    /// Loads the documentation page with a specified path from the "docs" in standard blob storage.
    /// </summary>
    /// <param name="path">The path of the page to load.</param>
    /// <param name="useCache"></param>
    /// <returns>The loaded documentation page if found, otherwise null.</returns>
    private async Task<string?> LoadDocsFromAzure(string path, bool useCache = false)
    {
        if (_blobServiceClient == null)
        {
            AddStatusMessage("BlobServiceClient is not initialized. Cannot load document from Azure.");
            return null;
        }
        try
        {
            var sasToken = _config["Media:SasToken"];
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            BlobClient blobClient = containerClient.GetBlobClient($"{path}");

            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadContentAsync();
                string documentContent = response.Value.Content.ToString();
                documentContent = MarkdownHelper.RemoveFrontMatter(documentContent);
                return documentContent;
            }
            else
            {
                AddStatusMessage($"Document not found in Azure Storage: {path}");
                return null;
            }
        }
        catch (Exception e)
        {
            AddStatusMessage($"Error loading {path} from Azure: {e.Message}");
            return null;
        }
    }

    public string BuildAbsoluteUrl(string relativePath)
    {
        string storageBaseUrl = _config["Media:StorageBaseUrl"] ?? $"https://fsdhstaticassetstorage.blob.core.windows.net/static/{ContainerName}/";
        if (relativePath.StartsWith("/"))
            relativePath = relativePath.TrimStart('/');

        return $"{storageBaseUrl}/{relativePath}";
    }

    private MemoryCacheEntryOptions GetEntryOptions() =>
             // Set the cache entry options
             new MemoryCacheEntryOptions()
                // Keep in cache for this time, reset time if accessed.
                .SetAbsoluteExpiration(DateTime.Now.AddHours(1));

    /// <summary>
    /// Retrieves the last commit timestamp for the repository.
    /// </summary>
    /// <param name="useCache">A boolean value indicating whether to use the cache.</param>
    /// <returns>The last commit timestamp if available, otherwise null.</returns>
    public async Task<DateTime?> LastRepoCommitTs(bool useCache = true)
    {
        if (_blobServiceClient == null)
        {
            _logger.LogError("BlobServiceClient is not initialized. Cannot retrieves the last timestamp.");
            return null;
        }
        try
        {
            var sasToken = _config["Media:SasToken"];
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

            // Construct the BlobClient URI with the SAS token
            var blobUri = new Uri($"{containerClient.Uri}/UserGuide/_sidebar.md?{sasToken}");
            BlobClient blobClient = containerClient.GetBlobClient($"UserGuide/_sidebar.md?{sasToken}");

            var properties = await blobClient.GetPropertiesAsync();

            return properties.Value.LastModified.UtcDateTime;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Cannot load last commit timestamp for user docs: {ex.Message}");
            return null;
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

public record TimeStampedStatus(DateTime Timestamp, string Message);
#nullable disable