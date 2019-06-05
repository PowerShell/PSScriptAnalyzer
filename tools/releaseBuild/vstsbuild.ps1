[cmdletbinding()]
param ( )

Begin
{
    $ErrorActionPreference = 'Stop'

    $gitBinFullPath = (Get-Command -Name git -CommandType Application).Path | Select-Object -First 1
    if ( ! $gitBinFullPath )
    {
        throw "Git is missing! Install from 'https://git-scm.com/download/win'"
    }

    # clone the release tools
    $releaseToolsDirName = "PSRelease"
    $releaseToolsLocation = Join-Path -Path $PSScriptRoot -ChildPath PSRelease
    if ( Test-Path $releaseToolsLocation )
    {
        Remove-Item -Force -Recurse -Path $releaseToolsLocation
    }
    & $gitBinFullPath clone -b master --quiet https://github.com/PowerShell/${releaseToolsDirName}.git $releaseToolsLocation
    Import-Module "$releaseToolsLocation/vstsBuild" -Force
    Import-Module "$releaseToolsLocation/dockerBasedBuild" -Force
}

End {

    $AdditionalFiles = .{
        Join-Path $PSScriptRoot -child "Image/buildPSSA.ps1"
        Join-Path $PSScriptRoot -child "Image/dockerInstall.psm1"
        }
    $buildPackageName = $null

    # defined if building in VSTS
    if($env:BUILD_STAGINGDIRECTORY)
    {
        # Use artifact staging if running in VSTS
        $destFolder = $env:BUILD_STAGINGDIRECTORY
    }
    else
    {
        # Use temp as destination if not running in VSTS
        $destFolder = $env:temp
    }

    $resolvedRepoRoot = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath "../../")).Path

    try
    {
        Write-Verbose "Starting build at $resolvedRepoRoot  ..." -Verbose
        Clear-VstsTaskState

        $buildArgs = @{
            RepoPath = $resolvedRepoRoot
            BuildJsonPath = './tools/releaseBuild/build.json'
            Parameters = @{ ReleaseTag = "unused" } # not needed for PSSA
            AdditionalFiles = $AdditionalFiles
            Name = "win7-x64"
        }
        Invoke-Build @buildArgs
    }
    catch
    {
        Write-VstsError -Error $_
    }
    finally{
        Write-VstsTaskState
        exit 0
    }
}
