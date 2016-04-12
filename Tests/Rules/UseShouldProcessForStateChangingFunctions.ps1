function Set-MyObject{ 
    [CmdletBinding(SupportsShouldProcess = $false)]
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