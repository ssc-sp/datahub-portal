$jsonFiles = gci -recurse -include *.json
$missingTranslations = @{};
foreach ($json in $jsonFiles) {
	Write-Host "Processing $json"
	$jsonContent = get-content $json.FullName | ConvertFrom-Json
	if (Get-Member -inputobject $jsonContent -name "Name" -Membertype Properties)
	{
		$missingTranslations[$jsonContent.Name] = $jsonContent.Name
	}
	if (Get-Member -inputobject $jsonContent -name "Description" -Membertype Properties)
	{
		$missingTranslations[$jsonContent.Description] = $jsonContent.Description
	}
}
$missingTranslations | ConvertTo-Json | Out-File "missingTranslations.json"