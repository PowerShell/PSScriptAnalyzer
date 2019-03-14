# escape=`
#0.3.6 (no powershell 6)
# FROM microsoft/windowsservercore
FROM microsoft/dotnet-framework:4.7.1
LABEL maintainer='PowerShell Team <powershellteam@hotmail.com>'
LABEL description="This Dockerfile for Windows Server Core with git installed via chocolatey."

SHELL ["C:\\windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe", "-command"]
# Install Git, and platyPS
# Git installs to C:\Program Files\Git
# nuget installs to C:\ProgramData\chocolatey\bin\NuGet.exe
COPY dockerInstall.psm1 containerFiles/dockerInstall.psm1

RUN Import-Module PackageManagement; `
    Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force; `
    Import-Module ./containerFiles/dockerInstall.psm1; `
    Install-ChocolateyPackage -PackageName git -Executable git.exe; `
    Install-ChocolateyPackage -PackageName nuget.commandline -Executable nuget.exe -Cleanup; `
    Install-Module -Force -Name platyPS; `
    Invoke-WebRequest -Uri https://raw.githubusercontent.com/dotnet/cli/master/scripts/obtain/dotnet-install.ps1 -outfile C:/dotnet-install.ps1; `
    C:/dotnet-install.ps1 -Channel Release -Version 2.1.4; `
    Add-Path C:/Users/ContainerAdministrator/AppData/Local/Microsoft/dotnet;

RUN Import-Module ./containerFiles/dockerInstall.psm1; `
#   git clone https://Github.com/PowerShell/PSScriptAnalyzer; `
    Install-ChocolateyPackage -PackageName dotnet4.5;

RUN Import-Module ./containerFiles/dockerInstall.psm1; `
    Install-ChocolateyPackage -PackageName netfx-4.5.2-devpack;

COPY buildPSSA.ps1 containerFiles/buildPSSA.ps1

ENTRYPOINT ["C:\\windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe", "-command"]

