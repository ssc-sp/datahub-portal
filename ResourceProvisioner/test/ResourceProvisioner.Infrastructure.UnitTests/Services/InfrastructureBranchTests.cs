using LibGit2Sharp;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.Infrastructure.UnitTests.Services;

/// <summary>
/// Runs the infrastructure branch flow against a local bare repository standing in for the Azure DevOps server:
/// fetch main only, branch from main, stage the workspace folder, push.
/// </summary>
[TestFixture]
public class InfrastructureBranchTests
{
    private const string Main = "main";
    private const string Workspace = "WS1";
    private const string ProjectPathPrefix = "terraform/projects";
    private const string WorkspaceFile = $"{ProjectPathPrefix}/{Workspace}/main.tf";
    private const string PipelineFile = "pipeline/resource-provisioner/templates/landingzone-pr.yaml";
    private static readonly Signature Author = new("Test", "test@example.com", DateTimeOffset.Now);

    private string _root = null!;
    private string _serverPath = null!;
    private string _seedPath = null!;
    private string _clonePath = null!;
    private Repository _seed = null!;

    [SetUp]
    public void SetUp()
    {
        _root = Path.Join(Path.GetTempPath(), $"infra-branch-{Guid.NewGuid():N}");
        _serverPath = Path.Join(_root, "server.git");
        _seedPath = Path.Join(_root, "seed");
        _clonePath = Path.Join(_root, "clone");

        Repository.Init(_serverPath, isBare: true);
        Repository.Init(_seedPath);
        _seed = new Repository(_seedPath);
        _seed.Network.Remotes.Add("origin", _serverPath);

        var first = CommitInSeed(PipelineFile, "version: old");
        var main = _seed.Branches.Add(Main, first, allowOverwrite: true);
        Commands.Checkout(_seed, main);
        PushSeed(Main);
    }

    [TearDown]
    public void TearDown()
    {
        _seed.Dispose();
        foreach (var file in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        Directory.Delete(_root, true);
    }

    [Test]
    public void CloneMainOnly_FetchesOnlyMain_WithItsFullHistory()
    {
        CommitInSeed(PipelineFile, "version: new");
        PushSeed(Main);
        PushSeedBranch(Workspace, _seed.Head.Tip);
        PushSeedBranch("OTHER", _seed.Head.Tip);

        InfrastructureBranch.CloneMainOnly(_serverPath, _clonePath, Main, new FetchOptions());

        using var clone = new Repository(_clonePath);
        Assert.Multiple(() =>
        {
            Assert.That(clone.Branches.Select(b => b.CanonicalName),
                Is.EquivalentTo(new[] { "refs/heads/main", "refs/remotes/origin/main" }));
            Assert.That(clone.Head.FriendlyName, Is.EqualTo(Main));
            Assert.That(clone.Commits.Count(), Is.EqualTo(2));
            Assert.That(File.ReadAllText(Path.Join(_clonePath, PipelineFile)), Is.EqualTo("version: new"));
        });
    }

    [Test]
    public void Push_WithNoBranchOnServer_CreatesIt()
    {
        var pushed = RunWorkspaceChange("# workspace change");

        Assert.That(ServerTip(Workspace), Is.EqualTo(pushed));
    }

    [Test]
    public void Push_WithLeftoverBranchAlreadyOnMain_MovesItForward_AndKeepsMainsFiles()
    {
        // The leftover branch points at an old main commit, like origin/PRTE1 did. It is never fetched, and the push
        // moves it forward.
        PushSeedBranch(Workspace, _seed.Head.Tip);
        CommitInSeed(PipelineFile, "version: new");
        PushSeed(Main);

        var pushed = RunWorkspaceChange("# workspace change");

        using var server = new Repository(_serverPath);
        var changed = server.Diff.Compare<TreeChanges>(server.Branches[Main].Tip.Tree, server.Branches[Workspace].Tip.Tree);
        Assert.Multiple(() =>
        {
            Assert.That(ServerTip(Workspace), Is.EqualTo(pushed));
            Assert.That(changed.Select(c => c.Path), Is.EqualTo(new[] { WorkspaceFile }));
        });
    }

    [Test]
    public void Push_WhenServerBranchHasCommitsNotOnMain_ThrowsAndLeavesTheServerBranchAlone()
    {
        // An open pull request: the server's branch has a commit that isn't on main yet.
        var openPullRequest = CommitOnSeedBranch(_seed.Head.Tip, WorkspaceFile, "# from the open pull request");
        PushSeedBranch(Workspace, openPullRequest);

        Assert.That(() => RunWorkspaceChange("# second change"),
            Throws.InvalidOperationException.With.Message.Contains("aren't on main yet"));
        Assert.That(ServerTip(Workspace), Is.EqualTo(openPullRequest.Sha));
    }

    [Test]
    public void StageWorkspaceFolder_StagesOnlyTheWorkspaceFolder_IncludingDeletes()
    {
        CommitInSeed($"{ProjectPathPrefix}/{Workspace}/old.tf", "# to delete");
        WriteSeedFile(WorkspaceFile, "# new file");
        File.Delete(Path.Join(_seedPath, $"{ProjectPathPrefix}/{Workspace}/old.tf"));
        WriteSeedFile(PipelineFile, "version: stray change");
        WriteSeedFile($"{ProjectPathPrefix}/{Workspace}0/main.tf", "# sibling folder");

        InfrastructureBranch.StageWorkspaceFolder(_seed, Workspace, ProjectPathPrefix);

        var status = _seed.RetrieveStatus();
        Assert.Multiple(() =>
        {
            Assert.That(status[WorkspaceFile].State, Is.EqualTo(FileStatus.NewInIndex));
            Assert.That(status[$"{ProjectPathPrefix}/{Workspace}/old.tf"].State, Is.EqualTo(FileStatus.DeletedFromIndex));
            Assert.That(status[PipelineFile].State, Is.EqualTo(FileStatus.ModifiedInWorkdir));
            Assert.That(status[$"{ProjectPathPrefix}/{Workspace}0/main.tf"].State, Is.EqualTo(FileStatus.NewInWorkdir));
        });
    }

    /// <summary>What a resource run does: fetch main only, branch, write and stage the workspace folder, commit, push.</summary>
    private string RunWorkspaceChange(string content)
    {
        if (Directory.Exists(_clonePath))
        {
            foreach (var file in Directory.EnumerateFiles(_clonePath, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            Directory.Delete(_clonePath, true);
        }

        InfrastructureBranch.CloneMainOnly(_serverPath, _clonePath, Main, new FetchOptions());
        using var clone = new Repository(_clonePath);
        InfrastructureBranch.CreateFromMain(clone, Workspace, Main);

        var path = Path.Join(_clonePath, WorkspaceFile);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        InfrastructureBranch.StageWorkspaceFolder(clone, Workspace, ProjectPathPrefix);
        var commit = clone.Commit("Workspace change", Author, Author);

        InfrastructureBranch.Push(clone, Workspace, new PushOptions());
        return commit.Sha;
    }

    private string ServerTip(string branch)
    {
        using var server = new Repository(_serverPath);
        return server.Branches[branch].Tip.Sha;
    }

    private void WriteSeedFile(string relativePath, string content)
    {
        var fullPath = Path.Join(_seedPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
    }

    private Commit CommitInSeed(string relativePath, string content)
    {
        WriteSeedFile(relativePath, content);
        Commands.Stage(_seed, relativePath);
        return _seed.Commit($"Change {relativePath}", Author, Author);
    }

    private Commit CommitOnSeedBranch(Commit start, string relativePath, string content)
    {
        var current = _seed.Head;
        var side = _seed.Branches.Add($"side-{Guid.NewGuid():N}", start);
        Commands.Checkout(_seed, side);
        var commit = CommitInSeed(relativePath, content);
        Commands.Checkout(_seed, current);
        _seed.Branches.Remove(side);
        return commit;
    }

    private void PushSeed(string branch) =>
        _seed.Network.Push(_seed.Network.Remotes["origin"], $"+refs/heads/{branch}:refs/heads/{branch}");

    private void PushSeedBranch(string serverBranch, Commit commit) =>
        _seed.Network.Push(_seed.Network.Remotes["origin"], $"+{commit.Sha}:refs/heads/{serverBranch}");
}
