param(
    [ValidateSet("net451", "netstandard1.6")]
    [string]$Framework = "net451",

    [ValidateSet("Debug", "Release", "PSv3Debug", "PSv3Release")]
    [string]$Configuration = "Debug"
)

$resourceScript = Join-Path $BuildRoot "New-StronglyTypedCsFileForResx.ps1"
$outPath = "$BuildRoot/out"
$modulePath = "$outPath/PSScriptAnalyzer"

$buildData = @{}
if ($BuildTask -eq "release") {
    $buildData = @{
        Frameworks = @{
            "net451"         = @{
                Configuration = @('Release', "PSV3Release")
            }
            "netstandard1.6" = @{
                Configuration = @('Release')
            }
        }
    }
}
else {
    $buildData.Add("Frameworks", @{})
    $buildData["Frameworks"].Add($Framework, @{})
    $buildData["Frameworks"][$Framework].Add("Configuration", $Configuration)
}

function CreateIfNotExists([string] $folderPath) {
    if (-not (Test-Path $folderPath)) {
        New-Item -Path $folderPath -ItemType Directory -Verbose:$verbosity
    }
}

function Get-BuildInputs($project) {
    pushd $buildRoot/$project
    gci -Filter *.cs
    gci -Directory -Exclude obj, bin | gci -Filter *.cs -Recurse
    popd
}

function Get-BuildOutputs($project) {
    $bin = "$buildRoot/$project/bin/$Configuration/$Framework"
    $obj = "$buildRoot/$project/obj/$Configuration/$Framework"
    if (Test-Path $bin) {
        gci $bin -Recurse
    }
    if (Test-Path $obj) {
        gci $obj -Recurse
    }
}

function Get-BuildTaskParams($project) {
    $taskParams = @{
        Data = $buildData
        Jobs = {
            $d = $($Task.Data)
            foreach ($frmwrk in $d.Frameworks.Keys) {
                foreach ($config in $d.Frameworks[$frmwrk].Configuration) {
					Write-Verbose -message "$config $framework" -Verbose:$true
                    dotnet build --framework $frmwrk --configuration $config
                }
            }
        }
    }

    $outputs = (Get-BuildOutputs $project)
    if ($null -ne $outputs) {
        $inputs = (Get-BuildInputs $project)
        $taskParams.Add("Outputs", $outputs)
        $taskParams.Add("Inputs", $inputs)
    }

    $taskParams
}

function Get-RestoreTaskParams($project) {
    @{
        Inputs  = "$BuildRoot/$project/project.json"
        Outputs = "$BuildRoot/$project/project.lock.json"
        Jobs    = {dotnet restore}
    }
}

function Get-CleanTaskParams($project) {
    @{
        Jobs = {
            if (Test-Path obj) {
                Remove-Item obj -Force -Recurse
            }

            if (Test-Path bin) {
                Remove-Item bin -Force -Recurse
            }
        }
    }
}

function Get-TestTaskParam($project) {
    @{
        Jobs = {
            Invoke-Pester
        }
    }
}

function Get-ResourceTaskParam($project) {
    @{
        Inputs  = "$project/Strings.resx"
        Outputs = "$project/Strings.cs"
        Jobs    = {& "$resourceScript $project"}
        Before  = "$project/build"
    }
}

function Add-ProjectTask([string]$project, [string]$taskName, [hashtable]$taskParams, [string]$pathPrefix = $buildRoot) {
    $jobs = [scriptblock]::Create(@"
pushd $pathPrefix/$project
$($taskParams.Jobs)
popd
"@)
    $taskParams.Jobs = $jobs
    $taskParams.Name = "$project/$taskName"
    task @taskParams
}

$projects = @("engine", "rules")

$projects | % {Add-ProjectTask $_ buildResource (Get-ResourceTaskParam $_)}
task buildResource -Before build "engine/buildResource", "rules/buildResource"

$projects | % {Add-ProjectTask $_ build (Get-BuildTaskParams $_)}
task build "engine/build", "rules/build"

$projects | % {Add-ProjectTask $_ "restore" (Get-RestoreTaskParams $_)}
task restore "engine/restore", "rules/restore"

$projects | % {Add-ProjectTask $_ clean (Get-CleanTaskParams $_)}
task clean "engine/clean", "rules/clean"

$projects | % {Add-ProjectTask $_ test (Get-TestTaskParam $_) "$BuildRoot/tests"}
task test "engine/test", "rules/test"

task makeModule {
    $solutionDir = $BuildRoot
    $itemsToCopyBinaries = @("$solutionDir\Engine\bin\$Configuration\$Framework\Microsoft.Windows.PowerShell.ScriptAnalyzer.dll",
        "$solutionDir\Rules\bin\$Configuration\$Framework\Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.dll")

    $itemsToCopyCommon = @("$solutionDir\Engine\PSScriptAnalyzer.psd1",
        "$solutionDir\Engine\PSScriptAnalyzer.psm1",
        "$solutionDir\Engine\ScriptAnalyzer.format.ps1xml",
        "$solutionDir\Engine\ScriptAnalyzer.types.ps1xml")

    $destinationDir = "$solutionDir\out\PSScriptAnalyzer"
    $destinationDirBinaries = $destinationDir
    if ($Framework -eq "netstandard1.6") {
        $destinationDirBinaries = "$destinationDir\coreclr"
    }
    elseif ($Configuration -match 'PSv3') {
        $destinationDirBinaries = "$destinationDir\PSv3"
    }

    Function CopyToDestinationDir($itemsToCopy, $destination) {
        CreateIfNotExists($destination)
        foreach ($file in $itemsToCopy) {
            Copy-Item -Path $file -Destination (Join-Path $destination (Split-Path $file -Leaf)) -Force
        }
    }

    CopyToDestinationDir $itemsToCopyCommon $destinationDir
    CopyToDestinationDir $itemsToCopyBinaries $destinationDirBinaries

    # Copy Settings File
    Copy-Item -Path "$solutionDir\Engine\Settings" -Destination $destinationDir -Force -Recurse

    # copy newtonsoft dll if net451 framework
    if ($Framework -eq "net451") {
        copy-item -path "$solutionDir\Rules\bin\$Configuration\$Framework\Newtonsoft.Json.dll" -Destination $destinationDirBinaries
    }
}

task cleanModule {
    Remove-Item -Path out/ -Recurse -Force
}


$docsPath = Join-Path $BuildRoot 'docs'
$outputDocsPath = Join-Path $modulePath 'en-US'
$bdInputs = {Get-ChildItem $docsPath -File -Recurse}
$bdOutputs = @(
        "$outputDocsPath/about_PSScriptAnalyzer.help.txt",
        "$outputDocsPath/Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml"
    )

# $buildDocsParams = @{
#     Inputs  = (Get-ChildItem $docsPath -File -Recurse)
#     Outputs = @(
#         "$outputDocsPath/about_PSScriptAnalyzer.help.txt",
#         "$outputDocsPath/Microsoft.Windows.PowerShell.ScriptAnalyzer.dll-Help.xml"
#     )
# }

task buildDocs -Inputs $bdInputs -Outputs $bdOutputs {
    # todo move common variables to script scope
    $markdownDocsPath = Join-Path $docsPath 'markdown'
    CreateIfNotExists($outputDocsPath)

    # copy the about help file
    Copy-Item -Path $docsPath\about_PSScriptAnalyzer.help.txt -Destination $outputDocsPath -Force

    # Build documentation using platyPS
    if ((Get-Module PlatyPS -ListAvailable -Verbose:$verbosity) -eq $null) {
        throw "Cannot find PlatyPS. Please install it from https://www.powershellgallery.com."
    }
    if ((Get-Module PlatyPS -Verbose:$verbosity) -eq $null) {
        Import-Module PlatyPS -Verbose:$verbosity
    }
    if (-not (Test-Path $markdownDocsPath -Verbose:$verbosity)) {
        throw "Cannot find markdown documentation folder."
    }
    New-ExternalHelp -Path $markdownDocsPath -OutputPath $outputDocsPath -Force
}

task cleanDocs -if (Test-Path $outputDocsPath) {
    Remove-Item -Path $outputDocsPath -Recurse -Force
}

task newSession {
    Start-Process "powershell" -ArgumentList @('-noexit', '-command "import-module c:\users\kabawany\source\repos\psscriptanalyzer\out\psscriptanalyzer"')
}
