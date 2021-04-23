function CompareWithNull {
    if ($null -eq $DebugPreference) {
    }
    if ($DebugPreference -eq $null) {
    }
}

$a = 3

if ($a -eq $null)
{
    if (3 -eq $null)
    {
    }
}