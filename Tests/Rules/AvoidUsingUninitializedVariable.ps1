# Script has uninitialized variables
# Must result in AvoidUsingUninitializedVariablerule violations along with other violations

function Test-MyPreference
{
    Write-Verbose $MyProgressPreference
    Write-Verbose $MyVerbosePreference
}

Write-Verbose $MyProgressPreference