using System.Text.Json;

namespace Datahub.Markdown;

public class DocumentationFileMapper
{
    private readonly FileMapping[] _mappings;

    public DocumentationFileMapper(string? content) 
    {
        if (content is null)
        {
            _mappings = Array.Empty<FileMapping>();
        }
        else
        {
            _mappings = JsonSerializer.Deserialize<FileMapping[]>(content) ?? Array.Empty<FileMapping>();
        }
    }

    public string? GetEnglishDocumentId(string url)
    {
        var mapping = _mappings.FirstOrDefault(m => url.Equals(m.En, StringComparison.OrdinalIgnoreCase));
        return mapping?.Id;
    }

    public string? GetFrenchDocumentId(string url)
    {
        var mapping = _mappings.FirstOrDefault(m => url.Equals(m.Fr, StringComparison.OrdinalIgnoreCase));
        return mapping?.Id;
    }
}

record FileMapping(string Id, string En, string Fr);