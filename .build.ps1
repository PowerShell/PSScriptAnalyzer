param(
    [ValidateSet("net451", "netstandard1.6")]
    [string]$Framework = "net451",

    [ValidateSet("Debug", "Release", "PSv3Debug", "PSv3Release")]
    [string]$Configuration = "Debug"
)

$resourceScript = Join-Path $BuildRoot "New-StronglyTypedCsFileForResx.ps1"

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
        Jobs = {dotnet build --framework $Framework --configuration $Configuration}
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
    } elseif ($Configuration -match 'PSv3') {
        $destinationDirBinaries = "$destinationDir\PSv3"
    }

    Function CopyToDestinationDir($itemsToCopy, $destination) {
        if (-not (Test-Path $destination)) {
            New-Item -ItemType Directory $destination -Force
        }
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
