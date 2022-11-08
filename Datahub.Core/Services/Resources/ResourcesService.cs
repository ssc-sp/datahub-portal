using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services.Resources
{
    public class ResourcesService : IResourcesService
    {
        private readonly string _wikiRoot;
        private readonly ILogger<ResourcesService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string WIKIROOT_CONFIG_KEY = "WikiURL";

        public ResourcesService(IConfiguration config, ILogger<ResourcesService> logger, IHttpClientFactory httpClientFactory)
        {
            _wikiRoot = config[WIKIROOT_CONFIG_KEY];
            _logger = logger;
            _httpClientFactory = httpClientFactory;
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

                //if (substitutions == null)
                //{
                //    return content;
                //}
                //else
                //{
                //    return substitutions
                //        .Aggregate(content, (current, s) => current.Replace(s.Item1, s.Item2));
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot load page url: {FullUrl}", fullUrl);
                return null;
            }

        }

        public async Task Test()
        {
            var barlw = await LoadSidebar();
            var ensb = await LoadSidebar(new string[] { "English" });
            var onbo = await LoadSidebar(new string[] { "English", "Onboarding" });
            var onbotoc = await LoadWikiPage("Onboarding-TOC.md", new string[] { "English", "Onboarding" });
            var notfound = await LoadWikiPage("Something-that-doesnt-exist.md", new string[] { "English" });

            var doc = Markdown.Parse(barlw);
            var descendants = doc.Descendants();

            var links = descendants.Where(d => d is LinkInline).Cast<LinkInline>();
            foreach (var l in links)
            {
                var ddd = $"url: {l.Url} - text: {l.FirstChild}";
                Console.WriteLine(ddd);
            }

            await Task.CompletedTask;
        }

        private static string BuildSidebarName(string[] folders = null)
        {
            StringBuilder sb = new();

            if (folders?.Length > 0)
            {
                foreach (var f in folders)
                {
                    sb.Append($"_{f}");
                }
            }

            sb.Append("_Sidebar.md");
            return sb.ToString();
        }

        private string BuildUrl(string name, string[] folders = null)
        {
            StringBuilder sb = new();

            sb.Append(_wikiRoot);
            if (folders?.Length > 0)
            {
                foreach (var f in folders)
                {
                    sb.Append($"{f}/");
                }
            }
            sb.Append(name);

            return sb.ToString();
        }

        private async Task<string> LoadSidebar(string[] folders = null)
        {
            var sbName = BuildSidebarName(folders);
            return await LoadWikiPage(sbName, folders);
        }

        private async Task<string> LoadWikiPage(string name, string[] folders = null)
        {
            var url = BuildUrl(name, folders);

            var httpClient = _httpClientFactory.CreateClient();
            try
            {
                return await httpClient.GetStringAsync(url);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error loading {url}", url);
                return await Task.FromResult(default(string));
            }
        }
    }
}
