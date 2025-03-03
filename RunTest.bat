@echo off
setlocal

REM Define the path to the test project
set testProjectPath=ServerlessOperations/test/Datahub.Functions.UnitTests/Datahub.Functions.UnitTests.csproj

REM Define the path to the coverage file
set coverageFilePath=ServerlessOperations/test/Datahub.Functions.UnitTests/TestResults/*/coverage.cobertura.xml

REM Delete all previous TestResults folders 
echo Deleting previous TestResults folders...
for /d %%x in (ServerlessOperations\test\Datahub.Functions.UnitTests\TestResults\*) do (
    echo Deleting %%x
    rmdir /s /q "%%x"
)

REM Run the tests and collect code coverage
echo Running tests and collecting code coverage...
dotnet test ServerlessOperations/test/Datahub.Functions.UnitTests/Datahub.Functions.UnitTests.csproj --collect:"XPlat Code Coverage"

REM Run the PowerShell script to remove the generated code from the coverage report
echo Cleaning up the coverage report...
powershell -File CleanCoverage.ps1 -coverageFilePath %coverageFilePath%

REM Generate the coverage report
echo Generating the coverage report...
reportgenerator -reports:ServerlessOperations/test/Datahub.Functions.UnitTests/TestResults/*/coverage.cobertura.xml -targetdir:ServerlessOperations/test/CodeCoverage -reporttypes:Html -assemblyfilters:+Datahub.Functions*

endlocal
echo Done.
pause