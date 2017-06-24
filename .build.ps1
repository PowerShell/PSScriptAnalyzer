param(
    [ValidateSet("net451", "netstandard1.6")]
    [string]$Framework = "net451",

    [ValidateSet("Debug", "Release", "PSv3Debug", "PSv3Release")]
    [string]$Configuration = "Debug"
)

task -Name restore `
    -Inputs Engine/project.json, Rules/project.json `
    -Outputs Engine/project.lock.json, Rules/project.lock.json `
    -Jobs {
    dotnet restore
}
