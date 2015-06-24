function CompareWithNull {
    if ($DebugPreference -eq $null) {
    }
}

if (@("dfd", "eee") -eq $null)
{
}

if ($randomUninitializedVariable -eq $null)
{
}

function Test
{
    $b = "dd", "ddfd";
    if ($b -ceq $null)
    {
        if ("dd","ee" -eq $null)
        {
        }
    }
}