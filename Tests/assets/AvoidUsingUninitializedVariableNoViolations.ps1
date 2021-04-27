# Script uses built-in preference variables
# Must not result in AvoidUsingUninitializedVariablerule violations
# However there are other violations in this script - Write-Verbose is not using positional parameters

function Test-Preference
{
    Write-Verbose $ProgressPreference
    Write-Verbose $VerbosePreference
}

Write-Verbose $ProgressPreference