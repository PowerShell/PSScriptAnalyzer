Function Get-SolutionPath
{
    Split-Path $PSScriptRoot -Parent
}

Function New-Release
{
    [CmdletBinding()]
    param($newVer, $oldVer)

    $isVersionGiven = $true
    if ($null -eq $newVer -or $null -eq $oldVer)
    {
        Write-Warning "Parameters are null. Checking changelog for version..."
        $isVersionGiven = $false
    }

    $solutionRoot = (Get-SolutionPath)

    $enginePath = Join-Path $solutionRoot "Engine"

    # Check if the changelog has entry for $newVer
    $moduleManifestPath = Join-Path $enginePath "PSScriptAnalyzer.psd1"
    $changelogPath = Join-Path $solutionRoot 'CHANGELOG.MD'
    $matches = [regex]::new("\[(\d+\.\d+\.\d+)\]").Matches((get-content $changelogPath -raw))
    $versions = $matches | ForEach-Object {$_.Groups[1].Value}
    if ($versions.Count -le 2)
    {
        throw "This edge condition for the number versions less that 2 is not implemented."
    }

    if ($isVersionGiven)
    {
        Function Test-IfNotPresentInChangelog
        {
            param($extractedVersion, $inputVersion)
            if ($extractedVersion -ne $inputVersion)
            {
                throw ("Version {0} does not exist in changelog. Please update changelog." -f $inputVersion)
            }
        }

        Test-IfNotPresentInChangelog $versions[0] $newVer
        Test-IfNotPresentInChangelog $versions[1] $oldVer
    }
    else
    {
        $newVer = $versions[0]
        $oldVer = $versions[1]
        $caption = "Version Check"
        $query = "Is version {0} the next release and version {1} the previous release ?" -f $newVer,$oldVer
        [bool] $yesToAll = $false
        [bool] $noToAll = $false

        if (!$PSCmdlet.ShouldContinue($query, $caption, $false, [ref] $yesToAll, [ref] $noToAll))
        {
            return "Aborting..."
        }
    }

    # update version
    Update-Version $newVer $oldVer $solutionRoot

    $changelogRegexPattern = "##\s\[{0}\].*\n((?:.*\n)+)##\s\[{1}\].*" `
                                -f [regex]::Escape($newVer),[regex]::Escape($oldVer)
    $changelogRegex = [regex]::new($changelogRegexPattern)
    $matches = $changelogRegex.Match((get-content $changelogPath -raw))
    $changelog = $matches.Groups[1].Value.Trim()

    Write-Verbose 'CHANGELOG'
    Write-Verbose $changelog

    $releaseNotesPattern = `
        "(?<releaseNotesBegin>ReleaseNotes\s*=\s*@')(?<releaseNotes>(?:.*\n)*)(?<releaseNotesEnd>'@)"
    $replacement = "`${releaseNotesBegin}" `
                    + [environment]::NewLine `
                    + $changelog `
                    + [environment]::NewLine `
                    + "`${releaseNotesEnd}"
    $r = [regex]::new($releaseNotesPattern)
    $updatedManifestContent = $r.Replace([System.IO.File]::ReadAllText($moduleManifestPath), $replacement)
    Set-ContentUtf8NoBom $moduleManifestPath $updatedManifestContent

    # build the module
    pushd $solutionRoot
    remove-item out/ -recurse -force
    dotnet restore
    .\buildCoreClr.ps1 -Framework net451 -Configuration Release -Build
    .\buildCoreClr.ps1 -Framework net451 -Configuration PSV3Release -Build
    .\buildCoreClr.ps1 -Framework netstandard1.6 -Configuration Release -Build
    .\build.ps1 -BuildDocs
    popd

}

function Combine-Path
{
    if ($args.Count -lt 2)
    {
        throw "give more than equal to 2 arguments"
    }

    $path = Join-Path $args[0] $args[1]
    for ($k = 2; $k -lt $args.Count; $k++)
    {
        $path = Join-Path $path $args[$k]
    }

    $path
}

function Update-Version
{
    param(
        [string] $newVer,
        [string] $oldVer,
        [string] $solutionPath
    )

    $ruleJson = Combine-Path $solutionPath 'Rules' 'project.json'
    $engineJson = Combine-Path $solutionPath 'Engine' 'project.json'
    $pssaManifest = Combine-Path $solutionPath 'Engine' 'PSScriptAnalyzer.psd1'

    Update-PatternInFile $ruleJson '"version": "{0}"' $oldVer $newVer
    Update-PatternInFile $ruleJson '"Engine": "{0}"' $oldVer $newVer
    Update-PatternInFile $engineJson '"version": "{0}"' $oldVer $newVer
    Update-PatternInFile $pssaManifest "ModuleVersion = '{0}'" $oldVer $newVer
}

function Update-PatternInFile
{
    param ($path, $unformattedPattern, $oldVal, $newVal)

    $content = Get-Content $path
    $newcontent = $content -replace ($unformattedPattern -f $oldVal),($unformattedPattern -f $newVal)
    Set-ContentUtf8NoBom $path $newcontent
}

function Set-ContentUtf8NoBom {
    param($path, $content)
    $utfNoBom = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllLines($path, $content, $utfNoBom)
}