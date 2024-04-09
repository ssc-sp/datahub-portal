using System.Text.Json.Nodes;
using Datahub.Core.Model.Repositories;

namespace Datahub.Core.Data.Databricks;

public class RepositoryInfoDto
{
    public string Id { get; set; }
    public string Path { get; set; }
    public string Url { get; set; }
    public string Provider { get; set; }
    public string Branch { get; set; }
    public string HeadCommitId { get; set; }

    public bool IsPublic { get; set; }

    public RepositoryInfoDto(JsonNode jsonNode)
    {
        Id = jsonNode["id"]?.ToString();
        Path = jsonNode["path"]?.ToString();
        Url = jsonNode["url"]?.ToString();
        Provider = jsonNode["provider"]?.ToString();
        Branch = jsonNode["branch"]?.ToString();
        HeadCommitId = jsonNode["head_commit_id"]?.ToString();
        IsPublic = false;
    }

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

    public string RepositoryName => Path.Split('/').Last();
}
