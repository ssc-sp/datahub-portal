using Datahub.Core.Services.Wiki;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IWikiService
{
    Task<string> LoadPage(string name, List<(string, string)> substitutions = null);
    Task<MarkdownLanguageRoot> LoadLanguageRoot(bool isFrench);
    Task<string> LoadResourcePage(MarkdownCard card);
    string GetEditUrl(MarkdownCard card);
    Task RefreshCache();
    IReadOnlyList<TimestampedResourceError> GetErrorList();
    Task LogNotFoundError(string pageName, string resourceRoot);
    Task LogNoArticleSpecifiedError(string url, string resourceRoot);
    event Func<Task> NotifyRefreshErrors;
}

public record TimestampedResourceError(DateTime Timestamp, string Message);