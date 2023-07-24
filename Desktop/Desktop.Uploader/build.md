
## With Debug
dotnet publish /restore /t:Publish /p:TargetFramework=net7.0-windows10.0.20348 /p:configuration=release /p:WindowsAppSDKSelfContained=true /p:Platform=x64 /p:PublishSingleFile=true /p:WindowsPackageType=None /p:RuntimeIdentifier=win10-x64 /p:PublishReadyToRun=true -v d

dotnet publish /restore /t:Publish /p:TargetFramework=net7.0-windows10.0.20348 /p:configuration=release /p:WindowsAppSDKSelfContained=true /p:Platform=x64 /p:PublishSingleFile=true /p:WindowsPackageType=None /p:RuntimeIdentifier=win10-x64 /p:PublishReadyToRun=false