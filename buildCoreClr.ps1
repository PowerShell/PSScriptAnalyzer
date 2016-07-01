param(
    [switch]$build,
    [switch]$uninstall,
    [switch]$install
)

$solutionDir = Split-Path $MyInvocation.InvocationName
if (-not (Test-Path "$solutionDir/global.json"))
{
    throw "Not in solution root"
}

$itemsToCopy = @("$solutionDir\Engine\bin\Debug\netcoreapp1.0\Microsoft.Windows.PowerShell.ScriptAnalyzer.dll",
    "$solutionDir\Rules\bin\Debug\netcoreapp1.0\Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.dll",
    "$solutionDir\Engine\PSScriptAnalyzer.psd1",
    "$solutionDir\Engine\PSScriptAnalyzer.psm1",
    "$solutionDir\Engine\ScriptAnalyzer.format.ps1xml",
    "$solutionDir\Engine\ScriptAnalyzer.types.ps1xml")

$destinationDir = "$solutionDir/out/coreclr/PSScriptAnalyzer"

if ($build)
{
    .\New-StronglyTypedCsFileForResx.ps1 Engine
    Push-Location Engine\
    dotnet build
    Pop-Location

    .\New-StronglyTypedCsFileForResx.ps1 Rules
    Push-Location Rules\
    dotnet build
    Pop-Location

    if (-not (Test-Path $destinationDir))
    {
        New-Item -ItemType Directory $destinationDir -Force
    }
    else
    {
        Remove-Item "$destinationDir\*" -Recurse
    }

    foreach ($file in $itemsToCopy)
    {
        Copy-Item -Path $file -Destination (Join-Path $destinationDir (Split-Path $file -Leaf)) -Verbose
    }
    (Get-Content "$destinationDir\PSScriptAnalyzer.psd1") -replace "ModuleVersion = '1.6.0'","ModuleVersion = '0.0.1'" | Out-File "$destinationDir\PSScriptAnalyzer.psd1" -Encoding ascii
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