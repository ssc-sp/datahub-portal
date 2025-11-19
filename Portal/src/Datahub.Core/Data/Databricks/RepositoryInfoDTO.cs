using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Datahub.Core.Model.Repositories;

namespace Datahub.Core.Data.Databricks;

public class RepositoryInfoDto
{
    public string? Id { get; set; }
    public string? Path { get; set; }
    public required string Url { get; set; }
    public required string Provider { get; set; }
    public required string Branch { get; set; }
    public string? HeadCommitId { get; set; }

    public bool IsPublic { get; set; }

    [SetsRequiredMembers]
    public RepositoryInfoDto(JsonNode jsonNode)
    {
        Id = jsonNode["id"]?.ToString();
        Path = jsonNode["path"]?.ToString();
        Url = jsonNode["url"]?.ToString() ?? throw new ArgumentNullException(nameof(Url));
        Provider = jsonNode["provider"]?.ToString() ?? throw new ArgumentNullException(nameof(Provider));
        Branch = jsonNode["branch"]?.ToString() ?? "master";
        HeadCommitId = jsonNode["head_commit_id"]?.ToString();
        IsPublic = false;
    }

    [SetsRequiredMembers]
    public RepositoryInfoDto(ProjectRepository projectRepository)
    {
        Id = projectRepository.Id.ToString();
        Path = projectRepository.Path;
        Url = projectRepository.RepositoryUrl;
        Provider = projectRepository.Provider;
        Branch = projectRepository.Branch;
        HeadCommitId = projectRepository.HeadCommitId;
        IsPublic = projectRepository.IsPublic;
    }

    public string? RepositoryName => Path?.Split('/').Last();
}
