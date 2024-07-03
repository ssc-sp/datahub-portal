using Datahub.Application.Configuration;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Octokit;

namespace Datahub.Portal.Controllers;

[Route("[controller]/[action]")]
public class GitController : Controller
{
    private readonly ILogger<GitController> _logger;
    private readonly DatahubPortalConfiguration _config;
    private readonly DatahubProjectDBContext _context;

    public GitController(ILogger<GitController> logger, DatahubPortalConfiguration config, DatahubProjectDBContext context)
    {
        _logger = logger;
        _config = config;
        _context = context;
    }

    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
    {
        try
        {
            var client = CreateGitHubClient();
            var result = await client.Oauth.CreateAccessToken(new OauthTokenRequest(_config.Github.ClientId, _config.Github.ClientSecret, code)
            {
                RedirectUri = new Uri(_config.Github.CallbackUrl)
            });
            await CreateRepoAsync(result.AccessToken, state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
        return Redirect($"/w/{state}");
    }

    private async Task CreateRepoAsync(string token, string acronym)
    {
        var client = CreateGitHubClient();
        client.Credentials = new Credentials(token);

        var prefix = _config.Github.RepoPrefix ?? "fsdh";
        var repoName = $"{prefix}-{acronym}".ToLower();

        var newRepo = new NewRepository(repoName)
        {
            Description = $"Workspace {acronym}'s repository / Référentiel de l'espace de travail {acronym}."
        };

        var repo = await client.Repository.Create(newRepo);

        await SaveProjectGitUrl(acronym, repo.CloneUrl);
    }

    private GitHubClient CreateGitHubClient()
    {
        return new GitHubClient(new ProductHeaderValue(_config.Github.AppName), new Uri("https://github.com/"));
    }

    private async Task SaveProjectGitUrl(string acronym, string url)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(e => e.Project_Acronym_CD == acronym);
        if (project is null)
            return;

        project.GitRepo_URL = url;

        await _context.SaveChangesAsync();
    }
}
