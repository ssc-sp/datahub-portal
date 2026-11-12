using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace ResourceProvisioner.Infrastructure.Services;

/// <summary>
/// Builds a workspace branch in the infrastructure repository and checks it before it is pushed.
/// A workspace branch must be the main branch plus changes to that workspace's own folder, and nothing else.
/// </summary>
internal static class InfrastructureBranch
{
    /// <summary>
    /// Checks out the workspace branch, starting from the tip of the main branch.
    /// The remote workspace branch is merged in only when it has commits that are not on main yet
    /// (a pull request still open). A leftover remote branch that main already contains is ignored, because
    /// merging an old snapshot can put older versions of files back on main.
    /// </summary>
    /// <remarks>Requires the full history of both branches, so the repository must not be a shallow clone.</remarks>
    public static void Prepare(Repository repo, string workspaceAcronym, string mainBranchName, Signature signature,
        ILogger logger)
    {
        var mainBranch = repo.Branches[mainBranchName]
                         ?? throw new InvalidOperationException(
                             $"Main branch {mainBranchName} does not exist in the infrastructure repository");

        var branch = repo.Branches[workspaceAcronym] ?? repo.CreateBranch(workspaceAcronym, mainBranch.Tip);
        Commands.Checkout(repo, branch);
        repo.Reset(ResetMode.Hard, mainBranch.Tip);
        logger.LogInformation("Branch {WorkspaceAcronym} checked out at {MainBranchName} ({Sha})", workspaceAcronym,
            mainBranchName, mainBranch.Tip.Sha);

        var remoteBranch = repo.Branches[$"origin/{workspaceAcronym}"];
        if (remoteBranch is null)
        {
            logger.LogInformation("No remote branch for {WorkspaceAcronym}", workspaceAcronym);
            return;
        }

        var divergence = repo.ObjectDatabase.CalculateHistoryDivergence(remoteBranch.Tip, mainBranch.Tip);
        if (divergence.CommonAncestor is null)
        {
            throw new InvalidOperationException(
                $"Remote branch {remoteBranch.FriendlyName} has no common history with {mainBranchName}");
        }

        if (divergence.AheadBy is null or 0)
        {
            logger.LogInformation(
                "Ignoring remote branch {RemoteBranch} ({Sha}): {MainBranchName} already contains all of its commits",
                remoteBranch.FriendlyName, remoteBranch.Tip.Sha, mainBranchName);
            return;
        }

        logger.LogInformation("Merging remote branch {RemoteBranch}: it has {Count} commits not on {MainBranchName}",
            remoteBranch.FriendlyName, divergence.AheadBy, mainBranchName);

        var result = repo.Merge(remoteBranch, signature, new MergeOptions
        {
            FailOnConflict = true,
            MergeFileFavor = MergeFileFavor.Normal
        });

        if (result.Status == MergeStatus.Conflicts)
        {
            throw new InvalidOperationException(
                $"Merging {remoteBranch.FriendlyName} into {workspaceAcronym} has conflicts; nothing was changed");
        }
    }

    /// <summary>
    /// Throws when the workspace branch changes any file outside the workspace's folder, compared with the main branch.
    /// </summary>
    public static void EnsureOnlyWorkspaceChanges(Repository repo, string workspaceAcronym, string mainBranchName,
        string projectPathPrefix)
    {
        var mainBranch = repo.Branches[mainBranchName]
                         ?? throw new InvalidOperationException(
                             $"Main branch {mainBranchName} does not exist in the infrastructure repository");
        var branch = repo.Branches[workspaceAcronym]
                     ?? throw new InvalidOperationException($"Branch {workspaceAcronym} does not exist");

        var workspaceFolder = $"{projectPathPrefix.Replace('\\', '/').Trim('/')}/{workspaceAcronym}/";
        var outsideFolder = repo.Diff.Compare<TreeChanges>(mainBranch.Tip.Tree, branch.Tip.Tree)
            .SelectMany(change => new[] { change.Path, change.OldPath })
            .Where(path => !string.IsNullOrEmpty(path))
            .Select(path => path.Replace('\\', '/'))
            // Case-insensitive: acronyms are unique regardless of case, and on Windows the files land in the existing
            // folder whatever its case.
            .Where(path => !path.StartsWith(workspaceFolder, StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToList();

        if (outsideFolder.Count > 0)
        {
            throw new InvalidOperationException(
                $"Refusing to push {workspaceAcronym}: it changes files outside {workspaceFolder}: " +
                string.Join(", ", outsideFolder.Take(20)) +
                (outsideFolder.Count > 20 ? $" and {outsideFolder.Count - 20} more" : string.Empty));
        }
    }
}
