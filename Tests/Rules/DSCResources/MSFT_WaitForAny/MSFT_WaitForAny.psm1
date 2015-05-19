#
# WaitForAny 
#

#
# The Get-TargetResource cmdlet.
#
function Get-TargetResource
{
    param
    (
        [parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string] $ResourceName,

        [parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string[]] $NodeName,

        [parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [PSCredential] $Credential,

        [ValidateRange(1,[Uint64]::MaxValue)]
        [Uint64] $RetryIntervalSec = 1, 

        [Uint32] $RetryCount = 0,

        [Uint32] $ThrottleLimit = 32 #PowerShell New-CimSession default throttle value
    )

    Import-Module $PSScriptRoot\..\..\PSDSCxMachine.psm1
    
    $b = @{"hash" = "table"}

    if ($true)
    {
        return $b;
    }
    elseif ($c)
    {
        return @{"hash2"="table2"}
    }
    else
    {
        # can't determine type of c so error should not be raised as we're trying to be conservative
        return $c;
    }
}

#
# The Set-TargetResource cmdlet.
#
function Set-TargetResource
{
    param
    (
        [parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string] $ResourceName,

        [parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string[]] $NodeName,

        [parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [PSCredential] $Credential,

        [ValidateRange(1,[Uint64]::MaxValue)]
        [Uint64] $RetryIntervalSec = 1, 

        [Uint32] $RetryCount = 0,

        [Uint32] $ThrottleLimit = 32 #PowerShell New-CimSession default throttle value
    )

    Import-Module $PSScriptRoot\..\..\PSDSCxMachine.psm1

    if ($PSBoundParameters["Verbose"])
    {
        PSDSCxMachine\Set-_InternalPSDscXMachineTR `
               -RemoteResourceId $ResourceName `
               -RemoteMachine $NodeName `
               -RemoteCredential $Credential `
               -MinimalNumberOfMachineInState 1 `
               -RetryIntervalSec $RetryIntervalSec `
               -RetryCount $RetryCount `
               -ThrottleLimit $ThrottleLimit `
               -Verbose
    }
    else
    {
        PSDSCxMachine\Set-_InternalPSDscXMachineTR `
               -RemoteResourceId $ResourceName `
               -RemoteMachine $NodeName `
               -RemoteCredential $Credential `
               -MinimalNumberOfMachineInState 1 `
               -RetryIntervalSec $RetryIntervalSec `
               -RetryCount $RetryCount `
               -ThrottleLimit $ThrottleLimit 
    }
}

# 
# Test-TargetResource
#
# 
function Test-TargetResource  
{
    param
    (
        [parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string] $ResourceName,

        [parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string[]] $NodeName,

        [parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [PSCredential] $Credential,

        [ValidateRange(1,[Uint64]::MaxValue)]
        [Uint64] $RetryIntervalSec = 1, 

        [Uint32] $RetryCount = 0,

        [Uint32] $ThrottleLimit = 32 #PowerShell New-CimSession default throttle value
    )

    Import-Module $PSScriptRoot\..\..\PSDSCxMachine.psm1

    $a = $true
    $a

    if ($true)
    {
        $false;
    }
    elseif ($b)
    {
        return $a -or $true
    }
    elseif ($c)
    {
        return $false;
    }
    else
    {
        return $true
    }
}



Export-ModuleMember -Function *-TargetResource