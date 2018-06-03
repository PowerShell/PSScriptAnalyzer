param(
    [switch]$Build,
    [switch]$Uninstall,
    [switch]$Install,

    [ValidateSet("net451", "netstandard2.0")]
    [string]$Framework = "netstandard2.0",

    [ValidateSet("Debug", "Release", "PSv3Debug", "PSv3Release", "PSv4Release")]
    [string]$Configuration = "Debug"
)

if ($Configuration -match "PSv" -and $Framework -eq "netstandard2.0")
{
    throw ("{0} configuration is not applicable to {1} framework" -f $Configuration,$Framework)
}

Write-Progress "Building ScriptAnalyzer"
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
if ($Framework -eq "netstandard2.0")
{
    $destinationDirBinaries = "$destinationDir\coreclr"
}
elseif ($Configuration -match 'PSv3') {
    $destinationDirBinaries = "$destinationDir\PSv3"
}
elseif ($Configuration -match 'PSv4') {
    $destinationDirBinaries = "$destinationDir\PSv4"
}

if ($build)
{
    Write-Progress "Building for framework $Framework, configuration $Configuration"
    Push-Location Rules\
    dotnet build Rules.csproj --framework $Framework --configuration $Configuration
    Pop-Location

    Function CopyToDestinationDir($itemsToCopy, $destination)
    {
        if (-not (Test-Path $destination))
        {
            $null = New-Item -ItemType Directory $destination -Force
        }
        foreach ($file in $itemsToCopy)
        {
            Copy-Item -Path $file -Destination (Join-Path $destination (Split-Path $file -Leaf)) -Force
        }
    }


    Write-Progress "Copying files to $destinationDir"
    CopyToDestinationDir $itemsToCopyCommon $destinationDir
    CopyToDestinationDir $itemsToCopyBinaries $destinationDirBinaries

    # Copy Settings File
    Copy-Item -Path "$solutionDir\Engine\Settings" -Destination $destinationDir -Force -Recurse

    # copy newtonsoft dll if net451 framework
    if ($Framework -eq "net451")
    {
        copy-item -path "$solutionDir\Rules\bin\$Configuration\$Framework\Newtonsoft.Json.dll" -Destination $destinationDirBinaries
    }
}

$modulePath = "$HOME\Documents\WindowsPowerShell\Modules";
$pssaModulePath = Join-Path $modulePath PSScriptAnalyzer


if ($uninstall)
{
    if ((Test-Path $pssaModulePath))
    {
        Remove-Item -Recurse $pssaModulePath
    }
}

if ($install)
{
    Write-Progress "Installing to $modulePath"
    Copy-Item -Recurse -Path "$destinationDir" -Destination "$modulePath\." -Force
}
