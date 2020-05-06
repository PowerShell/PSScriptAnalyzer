param(
    [Parameter()]
    [ValidateSet('Release', 'Debug')]
    [string]
    $Configuration = 'Debug',

    [Parameter()]
    [ValidateSet('netcoreapp3.1', 'net452')]
    [string[]]
    $TargetFramework = $(if ($IsWindows -eq $false) { 'netcoreapp3.1' } else { 'netcoreapp3.1', 'net452' })
)

$ErrorActionPreference = 'Stop'

$moduleName = "PSScriptAnalyzer"
$outLocation = "$PSScriptRoot/out"
$moduleOutPath = "$outLocation/$moduleName"

if (Test-Path $moduleOutPath)
{
    Remove-Item -Recurse -Force $moduleOutPath
}

Push-Location $PSScriptRoot
try
{
    foreach ($framework in $TargetFramework)
    {
        dotnet publish -f $framework

        if ($LASTEXITCODE -ne 0)
        {
            throw 'Dotnet publish failed'
        }

        New-Item -ItemType Directory -Path "$moduleOutPath/$framework"
        Copy-Item -Path "$PSScriptRoot/bin/$Configuration/$framework/publish/*.dll" -Destination "$moduleOutPath/$framework"
        Copy-Item -Path "$PSScriptRoot/bin/$Configuration/$framework/publish/*.pdb" -Destination "$moduleOutPath/$framework" -ErrorAction Ignore
    }
}
finally
{
    Pop-Location
}

Copy-Item -Path "$PSScriptRoot/$moduleName.psd1" -Destination $moduleOutPath