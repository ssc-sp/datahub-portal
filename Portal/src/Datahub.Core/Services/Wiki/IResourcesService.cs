using System.Collections.Generic;
using System.Threading.Tasks;

namespace Datahub.Core.Services.Wiki;

public interface IResourcesService
{
    Task<string> LoadPage(string name, List<(string, string)> substitutions = null);
    Task<ResourceLanguageRoot> LoadLanguageRoot(bool isFrench);
    Task<string> LoadResourcePage(ResourceCard card);
    string GetEditUrl(ResourceCard card);

    Task Test();
}