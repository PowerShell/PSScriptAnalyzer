Describe "Issue 828: No NullReferenceExceptionin AlignAssignmentStatement rule when CheckHashtable is enabled" {
    It "Should not throw" {
        # For details, see here: https://github.com/PowerShell/PSScriptAnalyzer/issues/828
        # The issue states basically that calling 'Invoke-ScriptAnalyzer .' with a certain settings file being in the same location that has CheckHashtable enabled
        # combined with a script contatining the command '$MyObj | % { @{$_.Name = $_.Value} }' could make it throw a NullReferencException.
        $cmdletThrewError = $false
        $initialErrorActionPreference = $ErrorActionPreference
        $initialLocation = Get-Location
        try
        {
            Set-Location $PSScriptRoot
            $ErrorActionPreference = 'Stop'
            Invoke-ScriptAnalyzer .
        }
        catch
        {
            $cmdletThrewError = $true
        }
        finally
        {
            $ErrorActionPreference = $initialErrorActionPreference
            Set-Location $initialLocation
        }
        
        $cmdletThrewError | Should -BeFalse
    }
}