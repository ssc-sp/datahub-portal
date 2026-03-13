param(
    [string]$BaseBranch = "origin/develop",
    [switch]$NoFetch
)

if (-not $NoFetch)
{
    git fetch --prune | Out-Null
}

$currentBranch = (git branch --show-current).Trim()
$protectedBranches = @("develop", "main", "master", $currentBranch)

$localBranches = @(git for-each-ref --format='%(refname:short)' refs/heads)
$deadBranches = @()

foreach ($branch in $localBranches)
{
    if ($protectedBranches -contains $branch)
    {
        continue
    }

    # A local branch is safe to delete when it is an ancestor of the base branch.
    git merge-base --is-ancestor $branch $BaseBranch
    if ($LASTEXITCODE -eq 0)
    {
        $deadBranches += $branch
    }
}

if ($deadBranches.Count -eq 0)
{
    Write-Host "No merged local branches found against '$BaseBranch'."
    exit 0
}

foreach ($branch in $deadBranches)
{
    Write-Host "Deleting branch '$branch'"
    git branch -d $branch
}
