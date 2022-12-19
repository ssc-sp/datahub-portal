using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services.Resources
{
    public interface IResourcesService
    {
        Task<string> LoadPage(string name, List<(string, string)> substitutions = null);
        Task<ResourceLanguageRoot> LoadLanguageRoot(bool isFrench);
        Task<string> LoadResourcePage(ResourceCard card);
        string GetEditUrl(ResourceCard card);
        Task RefreshCache();
        IReadOnlyList<TimestampedResourceError> GetErrorList();
        Task LogNotFoundError(string pageName, string resourceRoot);
        Task LogNoArticleSpecifiedError(string url, string resourceRoot);
        event Func<Task> NotifyRefreshErrors;
    }

    public record TimestampedResourceError(DateTime Timestamp, string Message);
}
