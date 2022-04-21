#!/usr/bin/env pwsh
#
# An example hook script to verify what is about to be committed.
# Called by "git commit" with no arguments.  The hook should
# exit with non-zero status after issuing an appropriate message if
# it wants to stop the commit.
#
# To enable this hook, rename this file to "pre-commit".
write-output "Pre-commit check for secrets"
write-output "======================================="

#$changes = git diff –name-only
$changes = git diff-index --name-only --cached --diff-filter=AM HEAD
$output = @()

function CheckSecrets($file)
{
	$fileData = Get-Content $winPath
	$secret = $fileData | Where-Object {$_ -like "*password*"}
	$secret += $fileData | Where-Object {$_ -like "*secret*"}    
    $secret += $fileData | Where-Object {$_ -like "*username*"}    
    return $secret    
}

foreach ($change in $changes)
{
    write-output "Running secret analyzer against: $change"
    $winPath = $change.replace("/", "\")
    $winPath = ".\$winPath"
    $secret = CheckSecrets($winPath)
    if ($secret -ne $null) {
        write-output "Found '$secret' in $winPath"
        $output += $secret
    }
}

write-output "======================================="

if ($output.Count -ne 0)
{
    Write-Output "Secrets were found in commit. fix or use git commit –no-verify"
    $output.Message
    exit 1
}