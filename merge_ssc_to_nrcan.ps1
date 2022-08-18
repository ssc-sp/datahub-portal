git remote add ssc https://github.com/ssc-sp/datahub-portal.git
#git fetch ssc
$branchName = "ssc_updates_$(get-date -Format "yyyyMMdd")"
git fetch ssc develop:$branchName