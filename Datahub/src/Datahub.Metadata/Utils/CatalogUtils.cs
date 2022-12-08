using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.Metadata.Utils;

public static class CatalogUtils
{
    public static List<CatalogObjectResult> GroupResults(List<CatalogObjectResult> results, CatalogObjectLanguage language)
    {
        // create a dict[groupId]
        var groups = new Dictionary<Guid, CatalogObjectResult>();
        foreach (var result in results)
        {
            if (result.GroupId.HasValue)
            {
                var key = result.GroupId.Value;
                if (groups.TryGetValue(key, out CatalogObjectResult current))
                {
                    if (result.Language == language && current.Language != language)
                        groups[key] = result;
                }
                else
                {
                    groups.Add(key, result);
                }
            }
        }
        return EnumerateResults(results, groups).ToList();
    }

    private static IEnumerable<CatalogObjectResult> EnumerateResults(List<CatalogObjectResult> results, Dictionary<Guid, CatalogObjectResult> groups)
    {
        foreach (var result in results)
        {
            if (result.GroupId.HasValue)
            {
                if (groups.TryGetValue(result.GroupId.Value, out CatalogObjectResult current) && current == result)
                    yield return result;
            }
            else
            {
                yield return result;
            }
        }
    }
}