$script:AnyProfileConfigKey = 'anyProfilePath'
$script:TargetProfileConfigKey = 'targetProfilePaths'

function New-CompatibleCmdletsSettings
{
    param(
        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]
        $AnyProfileUnionPath,

        [Parameter()]
        [string[]]
        $TargetProfilePaths
    )

    if (-not $TargetProfilePaths)
    {
        throw "Must specify at least one target profile"
    }

    $settingsContents = New-Object System.Text.StringBuilder

    $null = $settingsContents.Append('@{')
    $null = $settingsContents.Append("$script:AnyProfileConfigKey='$AnyProfileUnionPath';")
    $null = $settingsContents.Append("$script:TargetProfileConfigKey=@(")

    for ($i = 0; $i -lt $TargetProfilePaths.Count - 1; $i++)
    {
        $path = $TargetProfilePaths[$i]
        $null = $settingsContents.Append("'$path',")
    }
    $path = $TargetProfilePaths[$i]
    $null = $settingsContents.Append("'$path')}")

    return $settingsContents.ToString()
}