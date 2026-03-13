param(
    [string]$BaseBranch = "origin/develop",
    [switch]$NoFetch,
    [switch]$DeleteUnpushed
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
        continue
    }

    if ($DeleteUnpushed)
    {
        git rev-parse --abbrev-ref --symbolic-full-name "$branch@{upstream}" 1>$null 2>$null
        if ($LASTEXITCODE -ne 0)
        {
            $deadBranches += $branch
        }
    }
}

$deadBranches = @($deadBranches | Sort-Object -Unique)

if ($deadBranches.Count -eq 0)
{
    if ($DeleteUnpushed)
    {
        Write-Host "No merged or unpushed local branches found."
    }
    else
    {
        Write-Host "No merged local branches found against '$BaseBranch'."
    }
    exit 0
}

foreach ($branch in $deadBranches)
{
    Write-Host "Deleting branch '$branch'"
    git branch -D $branch
}
