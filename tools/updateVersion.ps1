# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param(
    [Parameter(Mandatory)]
    [semver]$Version,

    [Parameter(Mandatory)]
    [string]$Changes
)

git diff --staged --quiet --exit-code
if ($LASTEXITCODE -ne 0) {
    throw "There are staged changes in the repository. Please commit or reset them before running this script."
}

$Path = "Directory.Build.props"
$f = Get-Content -Path $Path
$f = $f -replace '^(?<prefix>\s+<ModuleVersion>)(.+)(?<suffix></ModuleVersion>)$', "`${prefix}${Version}`${suffix}"
$f | Set-Content -Path $Path
git add $Path

$Path = "docs/Cmdlets/PSScriptAnalyzer.md"
$f = Get-Content -Path $Path
$f = $f -replace '^(?<prefix>Help Version: )(.+)$', "`${prefix}${Version}"
$f | Set-Content -Path $Path
git add $Path

git commit --edit --message "v${Version}: $Changes"
