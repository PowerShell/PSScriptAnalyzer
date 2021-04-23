function Set-Service
{
    [CmdletBinding(SupportsShouldProcess = $true)]
    param ([string]$c)
}

function Stop-MyObject{ 
    [CmdletBinding(SupportsShouldProcess = 1)]
    param([string]$c, [int]$d) 

}

function New-MyObject{ 
    [CmdletBinding(SupportsShouldProcess = "a")]
    param([string]$c, [int]$d) 

}

function Test-GetMyObject{ 
    
    param([string]$c, [int]$d) 

} 

