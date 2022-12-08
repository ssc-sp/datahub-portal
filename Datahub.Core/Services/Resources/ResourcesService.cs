﻿using Markdig;
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

namespace Datahub.Core.Services.Resources
{
    public class ResourcesService : IResourcesService
    {
        private readonly string _wikiRoot;
        private readonly string _wikiEditPrefix;
        private readonly ILogger<ResourcesService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string WIKIROOT_CONFIG_KEY = "WikiURL";
        private const string WIKI_EDIT_URL_CONFIG_KEY = "EditWikiURLPrefix";

        //TODO use proper caching
        private ResourceLanguageRoot EnglishLanguageRoot;
        private ResourceLanguageRoot FrenchLanguageRoot;

        private IList<TimestampedResourceError> _errorList;

        public event Func<Task> NotifyRefreshErrors;

        public ResourcesService(IConfiguration config, ILogger<ResourcesService> logger, IHttpClientFactory httpClientFactory)
        {
            _wikiRoot = config[WIKIROOT_CONFIG_KEY];
            _wikiEditPrefix = config[WIKI_EDIT_URL_CONFIG_KEY];
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _errorList = new List<TimestampedResourceError>();
        }

        private async Task AddErrorMessage(string message)
        {
            var error = new TimestampedResourceError(DateTime.UtcNow, message);
            _errorList.Add(error);
            await InvokeNotifyRefreshErrors();
        }

        public async Task<string> LoadPage(string name, List<(string, string)> substitutions = null)
        {
            string nameTrimmed = name.TrimStart('/');


            var fullUrl = $"{_wikiRoot}{nameTrimmed}.md";
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

        private static IList<LinkInline> GetListedLinks(string inputMarkdown)
        {
            if (string.IsNullOrEmpty(inputMarkdown))
            {
                return default;
            }

            var doc = Markdown.Parse(inputMarkdown);
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

        private IList<string> GetPath(AbstractResource resource)
        {
            if (resource is null)
            {
                return new List<string>();
            }

            var parentPath = GetPath(resource.Parent);
            parentPath.Add(CleanupCharacters(resource.Title));
            return parentPath;
        }

        private async Task<ResourceCard> PopulateResourceCard(LinkInline link, ResourceCategory category)
        {
            var path = GetPath(category);
            var linkUrlMD = $"{link.Url}.md";
            var content = await LoadWikiPage(linkUrlMD, path);

            if (string.IsNullOrEmpty(content))
            {
                return await Task.FromResult(default(ResourceCard));
            }

            var cardDoc = Markdown.Parse(content);
            var cardDocFlattened = cardDoc.Descendants();
            
            var firstHeading = cardDocFlattened.FirstOrDefault(e => e is HeadingBlock) as HeadingBlock;
            var firstPara = cardDocFlattened.FirstOrDefault(e => e is ParagraphBlock) as ParagraphBlock;

            var title = firstHeading.Inline.FirstChild.ToString();
            var preview = firstPara.Inline.FirstChild.ToString();

            var card = new ResourceCard(title, preview, link.Url, category);

            return await Task.FromResult(card);
        }

        private async Task<ResourceCategory> PopulateResourceCategory(LinkInline link, ResourceLanguageRoot languageRoot)
        {
            var title = link.FirstChild.ToString();
            var category = new ResourceCategory(title, languageRoot);

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

        private async Task<ResourceLanguageRoot> PopulateResourceLanguageRoot(LinkInline link)
        {
            var title = link.FirstChild.ToString();
            var langRoot = new ResourceLanguageRoot(title);

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
            _errorList = new List<TimestampedResourceError>();

            await AddErrorMessage("Loading resources");

            var rootSidebar = await LoadSidebar();
            var rootLinks = GetListedLinks(rootSidebar);

            var enLink = rootLinks[0];
            var frLink = rootLinks[1];

            EnglishLanguageRoot = await PopulateResourceLanguageRoot(enLink);
            FrenchLanguageRoot = await PopulateResourceLanguageRoot(frLink);

            await AddErrorMessage("Finished loading resources");

            await Task.CompletedTask;
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

        public async Task<ResourceLanguageRoot> LoadLanguageRoot(bool isFrench)
        {
            if (EnglishLanguageRoot == null || FrenchLanguageRoot == null)
            {
                await LoadResourceTree();
            }

            var result = isFrench ? FrenchLanguageRoot : EnglishLanguageRoot;
            return await Task.FromResult(result);
        }

        public async Task<string> LoadResourcePage(ResourceCard card)
        {
            var path = GetPath(card.ParentCategory);
            var name = $"{card.Url}.md";
            return await LoadWikiPage(name, path);
        }

        public string GetEditUrl(ResourceCard card) => $"{_wikiEditPrefix}{card.Url}/_edit";

        public IReadOnlyList<TimestampedResourceError> GetErrorList() => _errorList.AsReadOnly();

        public async Task LogNotFoundError(string pageName, string resourceRoot) => await AddErrorMessage($"{pageName} was not found in {resourceRoot} cache");

        public async Task LogNoArticleSpecifiedError(string url, string resourceRoot) => await AddErrorMessage($"Embedded resource on page {url} does not specify a page name in {resourceRoot}");
    }
}
