[cmdletbinding()]
param(
    [Parameter(Mandatory=$True)]
    [ValidateSet('ssc','nrcan')]
    [string]$Source
)

git pull
switch ($Source)
{
    "ssc" {git remote add $Source https://github.com/ssc-sp/datahub-portal.git}
    "nrcan" {git remote add $Source https://github.com/NRCan/datahub-portal.git}
}

#git fetch $Source
$branchName = "$($Source)_updates_$(get-date -Format "yyyyMMdd")"
git fetch $Source develop:$branchName
git push origin $branchName
#try to merge develop into current branch
git checkout $branchName
$prLink = switch ($Source)
{
    "ssc" {"https://github.com/NRCan/datahub-portal/pull/new/$branchName"}
    "nrcan" {"https://github.com/ssc-sp/datahub-portal/pull/new/$branchName"}
}
git merge develop #likely to fail
git commit -m "merged recent develop changes"
git push
Start-Process $prLink
