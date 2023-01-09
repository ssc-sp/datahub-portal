using System;
using System.Linq;
using System.Text.Json;

namespace Datahub.Core.Services.Docs;

internal class DocumentationFileMapper
{
    private readonly FileMapping[] _mappings;

    public DocumentationFileMapper(string content) 
    {
        _mappings = JsonSerializer.Deserialize<FileMapping[]>(content) ?? Array.Empty<FileMapping>(); 
    }

    public string GetEnglishDocumentId(string url)
    {
        var mapping = _mappings.FirstOrDefault(m => url.Equals(m.En, StringComparison.OrdinalIgnoreCase));
        return mapping?.Id;
    }

    public string GetFrenchDocumentId(string url)
    {
        var mapping = _mappings.FirstOrDefault(m => url.Equals(m.Fr, StringComparison.OrdinalIgnoreCase));
        return mapping?.Id;
    }
}

record FileMapping(string Id, string En, string Fr);