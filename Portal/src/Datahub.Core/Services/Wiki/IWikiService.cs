using Datahub.Markdown.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Datahub.Core.Services.Wiki;

public interface IWikiService
{
    Task<string> LoadPage(string name, List<(string, string)> substitutions = null);
    Task<MarkdownLanguageRoot> LoadLanguageRoot(bool isFrench);
    Task<string> LoadResourcePage(MarkdownCard card);
    string GetEditUrl(MarkdownCard card);
    Task RefreshCache();
    IReadOnlyList<TimeStampedStatus> GetErrorList();
    Task LogNotFoundError(string pageName, string resourceRoot);
    Task LogNoArticleSpecifiedError(string url, string resourceRoot);
    event Func<Task> NotifyRefreshErrors;
}

public record TimeStampedStatus(DateTime Timestamp, string Message);