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
    gci -Directory -Exclude obj,bin | gci -Filter *.cs -Recurse
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
        Jobs    = {dotnet build --framework $Framework --configuration $Configuration}
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
        Inputs = "$BuildRoot/$project/project.json"
        Outputs = "$BuildRoot/$project/project.lock.json"
        Jobs = {dotnet restore}
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
        Inputs = "$project/Strings.resx"
        Outputs = "$project/Strings.cs"
        Jobs = {& "$resourceScript $project"}
        Before = "$project/build"
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

task createModule {

}
