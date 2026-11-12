using LibGit2Sharp;
using Microsoft.Extensions.Logging.Abstractions;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.Infrastructure.UnitTests.Services;

/// <summary>
/// Builds the workspace branch on local repositories that stand in for a full clone of the infrastructure repository.
/// The remote workspace branch is simulated with a refs/remotes/origin/&lt;acronym&gt; reference.
/// </summary>
[TestFixture]
public class InfrastructureBranchTests
{
    private const string Main = "main";
    private const string Workspace = "WS1";
    private const string ProjectPathPrefix = "terraform/projects";
    private const string PipelineFile = "pipeline/resource-provisioner/templates/landingzone-pr.yaml";
    private static readonly Signature Author = new("Test", "test@example.com", DateTimeOffset.Now);

    private string _path = null!;
    private Repository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        _path = Path.Combine(Path.GetTempPath(), $"infra-branch-{Guid.NewGuid():N}");
        Repository.Init(_path);
        _repo = new Repository(_path);

        var first = CommitFile(PipelineFile, "version: old");
        var main = _repo.Branches.Add(Main, first);
        Commands.Checkout(_repo, main);
        foreach (var other in _repo.Branches.Where(b => !b.IsRemote && b.FriendlyName != Main).ToList())
        {
            _repo.Branches.Remove(other);
        }
    }

    [TearDown]
    public void TearDown()
    {
        _repo.Dispose();
        foreach (var file in Directory.EnumerateFiles(_path, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        Directory.Delete(_path, true);
    }

    [Test]
    public void Prepare_WithNoRemoteBranch_StartsFromMain()
    {
        InfrastructureBranch.Prepare(_repo, Workspace, Main, Author, NullLogger.Instance);

        Assert.Multiple(() =>
        {
            Assert.That(_repo.Head.FriendlyName, Is.EqualTo(Workspace));
            Assert.That(_repo.Head.Tip.Sha, Is.EqualTo(_repo.Branches[Main].Tip.Sha));
        });
    }

    [Test]
    public void Prepare_WithLeftoverRemoteBranch_IgnoresItAndKeepsMainsFiles()
    {
        // The leftover branch points at an old main commit, like origin/PRTE1 did.
        SetRemoteBranch(_repo.Branches[Main].Tip);
        CommitFile(PipelineFile, "version: new");
        CommitFile($"{ProjectPathPrefix}/OTHER/main.tf", "# other workspace, new");

        InfrastructureBranch.Prepare(_repo, Workspace, Main, Author, NullLogger.Instance);

        Assert.Multiple(() =>
        {
            Assert.That(_repo.Head.Tip.Sha, Is.EqualTo(_repo.Branches[Main].Tip.Sha));
            Assert.That(ReadFile(PipelineFile), Is.EqualTo("version: new"));
            Assert.That(ReadFile($"{ProjectPathPrefix}/OTHER/main.tf"), Is.EqualTo("# other workspace, new"));
        });
    }

    [Test]
    public void Prepare_WithOpenPullRequestBranch_MergesItsUnmergedCommits()
    {
        var baseCommit = _repo.Branches[Main].Tip;
        var workspaceCommit = CommitOnSideBranch(baseCommit, $"{ProjectPathPrefix}/{Workspace}/main.tf", "# first change");
        SetRemoteBranch(workspaceCommit);
        CommitFile(PipelineFile, "version: new");

        InfrastructureBranch.Prepare(_repo, Workspace, Main, Author, NullLogger.Instance);

        Assert.Multiple(() =>
        {
            Assert.That(ReadFile($"{ProjectPathPrefix}/{Workspace}/main.tf"), Is.EqualTo("# first change"));
            Assert.That(ReadFile(PipelineFile), Is.EqualTo("version: new"));
            Assert.That(_repo.Head.Tip.Parents.Select(p => p.Sha),
                Is.EquivalentTo(new[] { _repo.Branches[Main].Tip.Sha, workspaceCommit.Sha }));
        });
    }

    [Test]
    public void Prepare_WhenRemoteBranchConflictsWithMain_Throws()
    {
        var baseCommit = _repo.Branches[Main].Tip;
        SetRemoteBranch(CommitOnSideBranch(baseCommit, PipelineFile, "version: branch"));
        CommitFile(PipelineFile, "version: main");

        Assert.That(() => InfrastructureBranch.Prepare(_repo, Workspace, Main, Author, NullLogger.Instance),
            Throws.InvalidOperationException.With.Message.Contains("conflicts"));
    }

    [Test]
    public void EnsureOnlyWorkspaceChanges_WithWorkspaceFolderChangesOnly_DoesNotThrow()
    {
        InfrastructureBranch.Prepare(_repo, Workspace, Main, Author, NullLogger.Instance);
        CommitFile($"{ProjectPathPrefix}/{Workspace}/main.tf", "# workspace change");

        Assert.That(() => InfrastructureBranch.EnsureOnlyWorkspaceChanges(_repo, Workspace, Main, ProjectPathPrefix),
            Throws.Nothing);
    }

    [Test]
    public void EnsureOnlyWorkspaceChanges_WithWorkspaceFolderInDifferentCase_DoesNotThrow()
    {
        InfrastructureBranch.Prepare(_repo, Workspace, Main, Author, NullLogger.Instance);
        CommitFile($"{ProjectPathPrefix}/{Workspace.ToLowerInvariant()}/main.tf", "# workspace change");

        Assert.That(() => InfrastructureBranch.EnsureOnlyWorkspaceChanges(_repo, Workspace, Main, ProjectPathPrefix),
            Throws.Nothing);
    }

    [Test]
    public void EnsureOnlyWorkspaceChanges_WithSharedFileChanged_Throws()
    {
        InfrastructureBranch.Prepare(_repo, Workspace, Main, Author, NullLogger.Instance);
        CommitFile($"{ProjectPathPrefix}/{Workspace}/main.tf", "# workspace change");
        CommitFile(PipelineFile, "version: older");

        Assert.That(() => InfrastructureBranch.EnsureOnlyWorkspaceChanges(_repo, Workspace, Main, ProjectPathPrefix),
            Throws.InvalidOperationException.With.Message.Contains(PipelineFile));
    }

    [Test]
    public void EnsureOnlyWorkspaceChanges_WithFolderThatOnlySharesThePrefix_Throws()
    {
        InfrastructureBranch.Prepare(_repo, Workspace, Main, Author, NullLogger.Instance);
        CommitFile($"{ProjectPathPrefix}/{Workspace}0/main.tf", "# another workspace");

        Assert.That(() => InfrastructureBranch.EnsureOnlyWorkspaceChanges(_repo, Workspace, Main, ProjectPathPrefix),
            Throws.InvalidOperationException.With.Message.Contains($"{Workspace}0/main.tf"));
    }

    private Commit CommitFile(string relativePath, string content)
    {
        var fullPath = Path.Combine(_path, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
        Commands.Stage(_repo, relativePath);
        return _repo.Commit($"Change {relativePath}", Author, Author);
    }

    private Commit CommitOnSideBranch(Commit start, string relativePath, string content)
    {
        var current = _repo.Head;
        var side = _repo.Branches.Add($"side-{Guid.NewGuid():N}", start);
        Commands.Checkout(_repo, side);
        var commit = CommitFile(relativePath, content);
        Commands.Checkout(_repo, current);
        _repo.Branches.Remove(side);
        return commit;
    }

    private void SetRemoteBranch(Commit commit) =>
        _repo.Refs.Add($"refs/remotes/origin/{Workspace}", commit.Id, allowOverwrite: true);

    private string ReadFile(string relativePath) => File.ReadAllText(Path.Combine(_path, relativePath));
}
