using LibGit2Sharp;

namespace ResourceProvisioner.Infrastructure.Services;

/// <summary>
/// Builds a workspace branch in the infrastructure repository: the main branch plus changes to that workspace's own
/// folder, and nothing else.
/// </summary>
/// <remarks>
/// Only the main branch is fetched. Workspace branches left on the server by earlier runs are never downloaded or
/// merged, so their older copies of files can't reach main.
/// </remarks>
internal static class InfrastructureBranch
{
    /// <summary>
    /// Creates a repository at <paramref name="path"/> with the full history of the main branch only, checked out.
    /// </summary>
    public static void CloneMainOnly(string url, string path, string mainBranchName, FetchOptions fetchOptions)
    {
        var refSpec = $"+refs/heads/{mainBranchName}:refs/remotes/origin/{mainBranchName}";

        Repository.Init(path);
        using var repo = new Repository(path);
        repo.Network.Remotes.Add("origin", url, refSpec);
        Commands.Fetch(repo, "origin", [refSpec], fetchOptions, null);

        var remoteMain = repo.Branches[$"origin/{mainBranchName}"]
                         ?? throw new InvalidOperationException(
                             $"Main branch {mainBranchName} was not found in the infrastructure repository");
        var main = repo.CreateBranch(mainBranchName, remoteMain.Tip);
        repo.Branches.Update(main, b => b.TrackedBranch = remoteMain.CanonicalName);
        Commands.Checkout(repo, main);
    }

    /// <summary>Creates and checks out the workspace branch at the tip of the main branch.</summary>
    public static void CreateFromMain(Repository repo, string workspaceAcronym, string mainBranchName)
    {
        var main = repo.Branches[mainBranchName]
                   ?? throw new InvalidOperationException(
                       $"Main branch {mainBranchName} does not exist in the infrastructure repository");
        var branch = repo.Branches[workspaceAcronym] ?? repo.CreateBranch(workspaceAcronym, main.Tip);
        Commands.Checkout(repo, branch);
        repo.Reset(ResetMode.Hard, main.Tip);
    }

    /// <summary>Stages the changes in the workspace's folder only, including deleted files.</summary>
    public static void StageWorkspaceFolder(Repository repo, string workspaceAcronym, string projectPathPrefix)
    {
        Commands.Stage(repo, WorkspaceFolder(projectPathPrefix, workspaceAcronym) + "*");
    }

    /// <summary>The workspace's folder in the repository, e.g. terraform/projects/ABC/.</summary>
    public static string WorkspaceFolder(string projectPathPrefix, string workspaceAcronym) =>
        $"{projectPathPrefix.Replace('\\', '/').Trim('/')}/{workspaceAcronym}/";

    /// <summary>
    /// Pushes the workspace branch with a normal (not forced) push. The server accepts it when its copy of the branch
    /// is missing or already on main; it rejects it when the branch has commits that aren't on main yet, which means an
    /// infrastructure pull request for the workspace is still open.
    /// </summary>
    public static void Push(Repository repo, string workspaceAcronym, PushOptions options)
    {
        var branch = repo.Branches[workspaceAcronym]
                     ?? throw new InvalidOperationException($"Branch {workspaceAcronym} does not exist");
        var remote = repo.Network.Remotes["origin"];
        repo.Branches.Update(branch, b => b.Remote = remote.Name, b => b.UpstreamBranch = branch.CanonicalName);

        string? rejection = null;
        options.OnPushStatusError = error => rejection = error.Message;
        try
        {
            repo.Network.Push(repo.Branches[workspaceAcronym], options);
        }
        catch (LibGit2SharpException e) when (e.Message.Contains("fast", StringComparison.OrdinalIgnoreCase) ||
                                               e.Message.Contains("not present locally", StringComparison.OrdinalIgnoreCase))
        {
            rejection = e.Message;
        }

        if (rejection is not null)
        {
            throw new InvalidOperationException(
                $"The {workspaceAcronym} branch in the infrastructure repository has commits that aren't on main yet, " +
                $"usually from an infrastructure pull request that is still open. Complete or abandon that pull request " +
                $"and delete the {workspaceAcronym} branch, then retry. Git: {rejection}");
        }
    }
}
