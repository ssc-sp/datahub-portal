# Define the directory to start the search
$startDirectory = "."

# Define the output file
$outputFile = "log_lines.txt"

# Clear the output file if it exists
if (Test-Path $outputFile) {
    Remove-Item $outputFile
}

# Get all .cs and .razor files recursively
$files = Get-ChildItem -Path $startDirectory -Recurse -Include *.cs, *.razor

# Initialize an array to store log lines
$logLines = @()

# Loop through each file
foreach ($file in $files) {
    # Read the file content
    Write-Host "Processing $($file.FullName)"
    $content = Get-Content -Path $file.FullName

    # Ensure content is not null or whitespace
    if (![string]::IsNullOrWhiteSpace($content)) {
        # Find multiline log statements containing .Log and ending with );
        $lMatches = [regex]::Matches($content, "(?ms)(?<=^|\s)\S*\.Log\w+\(.*?\);")

        foreach ($match in $lMatches) {
            if ($match.Value -notmatch "using ") {
                $cleanedLine = $match.Value -replace "\s+", " "

                # Extract severity level
                $severity = [regex]::Match($cleanedLine, "\.Log(\w+)\(").Groups[1].Value     
                
                # Format the output with severity level and log line
                $formattedLine = "$($file.FullName)`t$severity`t$cleanedLine".Trim()                
                $logLines += $formattedLine
            }
        }
    }
}

# Write the log lines to the output file
$logLines | Out-File -FilePath $outputFile

Write-Host "Log lines have been captured in $outputFile"
