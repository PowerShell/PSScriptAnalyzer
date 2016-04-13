function Set-MyObject{ 
    [CmdletBinding(SupportsShouldProcess = $false)]
    param([string]$c, [int]$d) 

} 

function Stop-MyObject{ 
    [CmdletBinding(SupportsShouldProcess = 0)]
    param([string]$c, [int]$d) 

}

function New-MyObject{ 
    [CmdletBinding(SupportsShouldProcess = "")]
    param([string]$c, [int]$d) 

}

function Set-MyObject{ 
    [CmdletBinding()]
    param([string]$c, [int]$d) 

} 

function Remove-MyObject{
    [CmdletBinding()]
    param([string]$c, [int]$d) 
}