param (
    [string]$env,
    [string]$headless='false',
    [int]$slowmo=2000
)

# We remember the previous working directory and set the working directory to the root of the Datahub.Specs project
$prevPwd = $PWD; Set-Location -ErrorAction Stop -LiteralPath $PSScriptRoot/..

try {
    
    # We figure out the proper url to use for the tests based on the environment
    $url = switch ( $env )
    {
        'dev' {"https://fsdh-portal-app-dev.azurewebsites.net"}
        'poc' {"https://federal-science-datahub.canada.ca"}
        'int' {"https://fsdh-portal-app-int.azurewebsites.net"}
        'local' {"http://localhost:5001" }
        default {"http://localhost:5001"}
    }
    
    # We start the tests with the given parameters
    echo "Running tests for $env environment at $url"
    dotnet test --environment TEST_URL=$url --environment HEADLESS=$headless --environment SLOWMO=$slowmo --environment SCRIPT_RUN=$true
}
finally {
    # We end the tests by returning to the previous working directory
    $prevPwd | Set-Location
}
