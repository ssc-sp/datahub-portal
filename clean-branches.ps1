$dead_branches = @(git branch --merged origin/develop)
foreach ($sql in $dead_branches)
{
    $clean = $sql.Trim()
    Write-Host "Deleting branch '$clean'"
    git branch -d $clean
}
