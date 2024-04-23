# Takes an appsettings file and converts it to a json file that can be used in .net apps
# [
#     {
#       "name": "Achievements__Enabled",
#       "value": "false",
#       "slotSetting": false
#     },...
# ]

$CurrentPath = Split-Path -Parent $MyInvocation.MyCommand.Path

#get full path from $CurrentPath

Import-Module $CurrentPath/../scripts/appsettings.psm1 -Force

$appServiceSettings = Get-Content "appservice.json" | ConvertFrom-Json
# convert $appServiceSettings to a hashtable
$hashtable = @{}
$appServiceSettings | ForEach-Object { $hashtable[$_.Name] = $_.Value }

$hierarchichal = ConvertFrom-HashTable $hashtable -Separator "__"
$hierarchichal | ConvertTo-Json -Depth 100
