# Setting up a workstation to develop with FSDH

## Step 1: Install WSL 2

WSL is part of the standard catalog of SSC applications. It can be requested through the `Employee Service Portal`. WSL 2 provides a complete Linux environment on Windows, which is essential for running the development tools and scripts used in this project. Once installed, you'll have access to a Linux terminal where you can run bash commands and install Linux packages needed for development.

## Step 2: Install Powershell

PowerShell is required for running the configuration and build scripts in this project. Install PowerShell Core on your WSL environment to ensure compatibility with the project's automation scripts. You can install it using the package manager or download it directly from Microsoft's official repository.

## Step 3: Install Dotnet 9

The DataHub Portal is built using .NET 9, so you'll need the latest SDK to compile and run the application. The commands below will add Microsoft's package repository and install the .NET 9 SDK with all necessary tools for development, including the runtime, libraries, and command-line tools.

```bash
sudo add-apt-repository ppa:dotnet/backports
sudo apt install dotnet-sdk-9.0
```

## Step 4: Populate settings

This step generates all the necessary configuration files and settings required for local development. The script will create template configuration files with default values that you can customize for your development environment. Make sure you're in the root directory of the project before running this command.

```bash
./gen-all-settings.ps1
```