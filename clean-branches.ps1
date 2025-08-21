param(
    [string]$BaseBranch = 'origin/develop',
    [switch]$Force,          # Force delete non‑merged (uses -D)
    [switch]$NoFetch         # Skip fetch --prune
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $NoFetch) {
    Write-Host "Fetching & pruning remotes..." -ForegroundColor Cyan
    git fetch --all --prune | Out-Null
}

# # Validate base branch existence
# if (-not (git show-ref --verify --quiet "refs/remotes/$BaseBranch")) {
#     Write-Host "Base branch '$BaseBranch' not found. Aborting." -ForegroundColor Red
#     exit 1
# }

# Helper: clean a branch name (strip leading * and whitespace)
function Normalize-Branch {
    param($line)
    $line.Trim() -replace '^\*','' -replace '^\s+',''
}

# Collect branches already merged into the chosen base (safe to delete with -d)
$merged = git branch --merged $BaseBranch |
    ForEach-Object { Normalize-Branch $_ } |
    Where-Object {
        $_ -and $_ -notin @('main','master','develop','release','proof-of-concept') # protect common primaries
    }

# Collect local branches whose upstream is gone (git branch -vv marks them with [gone])
$gone = git branch -vv |
    ForEach-Object {
        # Example line: "  feature/foo  a1b2c3d [origin/feature/foo: gone] Commit msg"
        # or:          "  feature/foo  a1b2c3d [gone] Commit msg"
        if ($_ -match '^\*?\s*(\S+)\s+[0-9a-fA-F]+\s+\[gone\]') {
            $matches[1]
        } elseif ($_ -match '^\*?\s*(\S+)\s+[0-9a-fA-F]+\s+\[origin\/[^\]]+: gone\]') {
            $matches[1]
        }
    } |
    Where-Object {
        $_ -and $_ -notin @('main','master','develop','release','proof-of-concept')
    }

# Exclude current branch from any deletion set
$current = (git branch --show-current).Trim()
$merged = $merged | Where-Object { $_ -ne $current }
$gone   = $gone   | Where-Object { $_ -ne $current }

# Merge lists (keep uniqueness) but track category
$merged = $merged ?? @()
$merged = $merged | ForEach-Object { [string]$_ }
$mergedList = [System.Collections.Generic.List[string]]::new()
$merged | ForEach-Object { $mergedList.Add($_) }
$mergedSet = [System.Collections.Generic.HashSet[string]]::new($mergedList)
$goneUnique = $gone | Where-Object { -not $mergedSet.Contains($_) }

if (-not $merged -and -not $goneUnique) {
    Write-Host "Nothing to clean." -ForegroundColor Green
    exit 0
}

Write-Host "`nMerged branches (safe delete with -d):" -ForegroundColor Yellow
$merged | ForEach-Object { Write-Host "  $_" }

Write-Host "`nOrphaned local branches (upstream gone):" -ForegroundColor Yellow
$goneUnique | ForEach-Object { Write-Host "  $_" }

Write-Host ""
$confirm = Read-Host "Delete these branches? (y/N)"
if ($confirm -notin @('y','Y','yes','YES')) {
    Write-Host "Aborted."
    exit 0
}

foreach ($b in $merged) {
    Write-Host "Deleting merged branch '$b'"
    git branch -d -- $b
}

foreach ($b in $goneUnique) {
    $flag = if ($Force) { '-D' } else { '-d' }
    Write-Host "Deleting orphaned branch '$b' (upstream gone) with $flag"
    # If not merged and not using -Force, deletion may fail (expected).
    git branch $flag -- $b
}

Write-Host "`nDone." -ForegroundColor Green