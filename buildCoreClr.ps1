param(
    [switch]$Build,
    [switch]$Uninstall,
    [switch]$Install,

    [ValidateSet("net451", "netstandard1.6")]
    [string]$Framework = "netstandard1.6",

    [ValidateSet("Debug", "Release", "PSv3Debug", "PSv3Release")]
    [string]$Configuration = "Debug"
)

if ($Configuration -match "PSv3" -and $Framework -eq "netstandard1.6")
{
    throw ("{0} configuration is not applicable to {1} framework" -f $Configuration,$Framework)
}

Function Test-DotNetRestore
{
    param(
        [string] $projectPath
    )
    Test-Path (Join-Path $projectPath 'project.lock.json')
}

$solutionDir = Split-Path $MyInvocation.InvocationName
if (-not (Test-Path "$solutionDir/global.json"))
{
    throw "Not in solution root"
}

$itemsToCopyBinaries = @("$solutionDir\Engine\bin\$Configuration\$Framework\Microsoft.Windows.PowerShell.ScriptAnalyzer.dll",
    "$solutionDir\Rules\bin\$Configuration\$Framework\Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.dll")

$itemsToCopyCommon = @("$solutionDir\Engine\PSScriptAnalyzer.psd1",
    "$solutionDir\Engine\PSScriptAnalyzer.psm1",
    "$solutionDir\Engine\ScriptAnalyzer.format.ps1xml",
    "$solutionDir\Engine\ScriptAnalyzer.types.ps1xml")

$destinationDir = "$solutionDir\out\PSScriptAnalyzer"
$destinationDirBinaries = $destinationDir
if ($Framework -eq "netstandard1.6")
{
    $destinationDirBinaries = "$destinationDir\coreclr"
}
elseif ($Configuration -match 'PSv3') {
    $destinationDirBinaries = "$destinationDir\PSv3"
}


if ($build)
{

    if (-not (Test-DotNetRestore((Join-Path $solutionDir Engine))))
    {
        throw "Please restore project Engine"
    }
    .\New-StronglyTypedCsFileForResx.ps1 Engine
    Push-Location Engine\
    dotnet build --framework $Framework --configuration $Configuration
    Pop-Location


    if (-not (Test-DotNetRestore((Join-Path $solutionDir Rules))))
    {
        throw "Please restore project Rules"
    }
    .\New-StronglyTypedCsFileForResx.ps1 Rules
    Push-Location Rules\
    dotnet build --framework $Framework --configuration $Configuration
    Pop-Location

    Function CopyToDestinationDir($itemsToCopy, $destination)
    {
        if (-not (Test-Path $destination))
        {
            New-Item -ItemType Directory $destination -Force
        }
        foreach ($file in $itemsToCopy)
        {
            Copy-Item -Path $file -Destination (Join-Path $destination (Split-Path $file -Leaf)) -Verbose -Force
        }
    }
    CopyToDestinationDir $itemsToCopyCommon $destinationDir
    CopyToDestinationDir $itemsToCopyBinaries $destinationDirBinaries

    # Copy Settings File
    Copy-Item -Path "$solutionDir\Engine\Settings" -Destination $destinationDir -Force -Recurse -Verbose

    # copy newtonsoft dll if net451 framework
    if ($Framework -eq "net451")
    {
        copy-item -path "$solutionDir\Rules\bin\$Configuration\$Framework\Newtonsoft.Json.dll" -Destination $destinationDirBinaries -Verbose
    }
}

$modulePath = "$HOME\Documents\WindowsPowerShell\Modules";
$pssaModulePath = Join-Path $modulePath PSScriptAnalyzer


if ($uninstall)
{
    if ((Test-Path $pssaModulePath))
    {
        Remove-Item -Recurse $pssaModulePath -Verbose
    }
}

if ($install)
{
    Copy-Item -Recurse -Path "$destinationDir" -Destination "$modulePath\." -Verbose -Force
}