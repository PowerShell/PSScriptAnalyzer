#
# WaitForAll
#

#
# The Get-TargetResource cmdlet.
#
function NotGet-TargetResource
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

        [Uint32] $ThrottleLimit = 32 #Powershell New-CimSession default throttle value
    )


    Import-Module $PSScriptRoot\..\..\PSDSCxMachine.psm1

    return PSDSCxMachine\Get-_InternalPSDscXMachineTR `
               -RemoteResourceId $ResourceName `
               -RemoteMachine $NodeName `
               -RemoteCredential $Credential `
               -MinimalNumberOfMachineInState $NodeName.Count `
               -RetryIntervalSec $RetryIntervalSec `
               -RetryCount $RetryCount `
               -ThrottleLimit $ThrottleLimit
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

        [Uint32] $ThrottleLimit = 32 #Powershell New-CimSession default throttle value
    )

    Import-Module $PSScriptRoot\..\..\PSDSCxMachine.psm1

    if ($PSBoundParameters["Verbose"])
    {
        PSDSCxMachine\Set-_InternalPSDscXMachineTR `
               -RemoteResourceId $ResourceName `
               -RemoteMachine $NodeName `
               -RemoteCredential $Credential `
               -MinimalNumberOfMachineInState $NodeName.Count `
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
               -MinimalNumberOfMachineInState $NodeName.Count `
               -RetryIntervalSec $RetryIntervalSec `
               -RetryCount $RetryCount `
               -ThrottleLimit $ThrottleLimit 
    }

    $true -and $false
    
    if ($true) {
        return 4;
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

        [ValidateRange(1,[Uint64]::MaxValue)]
        [Uint64] $RetryIntervalSec = 1, 

        [Uint32] $RetryCount = 0,

        [Uint32] $ThrottleLimit = 32 #Powershell New-CimSession default throttle value
    )

    Import-Module $PSScriptRoot\..\..\PSDSCxMachine.psm1

    $b = @{"Test"=3}
    $b

    $c = [Math]::Sin(3)
    $c

    $d = [bool[]]@($true, $false)
    
    $d

    foreach ($d in $b)
    {
         $test
    }

    return $true
}



Export-ModuleMember -Function *-TargetResource