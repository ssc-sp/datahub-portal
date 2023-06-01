using System.Linq;
using System.Text.Json.Nodes;

namespace Datahub.Core.Data.Databricks;

public record RepositoryInfo
{
    public string Id { get; set; }
    public string Path { get; set; }
    public string Url { get; set; }
    public string Provider { get; set; }
    public string Branch { get; set; }
    public string HeadCommitId { get; set; }
    
    public bool IsPublic { get; set; }

    public RepositoryInfo(JsonNode jsonNode)
    {
        Id = jsonNode["id"]?.ToString();
        Path = jsonNode["path"]?.ToString();
        Url = jsonNode["url"]?.ToString();
        Provider = jsonNode["provider"]?.ToString();
        Branch = jsonNode["branch"]?.ToString();
        HeadCommitId = jsonNode["head_commit_id"]?.ToString();
        IsPublic = false;
    }
    
    public string RepositoryName => Path.Split('/').Last();
}