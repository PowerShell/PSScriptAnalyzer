Write-Output -InputObject "Use positional parameters plz!"
Get-ChildItem -Path "Tests"
Clear-Host
Split-Path -Path "Random" -leaf
Get-Process | Where-Object {$_.handles -gt 200}
get-service-computername localhost | where {($_.status -eq "Running") -and ($_.CanStop -eq $true)}
1, 2, $null, 4 | ForEach-Object {"Hello"}
& "$env:Windir\System32\Calc.exe" Parameter1 Parameter2

# There was a bug in Positional Parameter rule that resulted in the rule being fired 
# when using external application with absolute paths
# The below function is to validate the fix - rule must not get triggered
function TestExternalApplication
{    
    & "c:\Windows\System32\Calc.exe" parameter1
}

# less than 3 arguments so rule won't trigger
Get-Content Test
Get-ChildItem Tests
Write-Output "I don't want to use positional parameters"
Split-Path "RandomPath" -Leaf
Get-Process | ForEach-Object {Write-Host $_.name -foregroundcolor cyan}

$srvc = Get-WmiObject Win32_Service -ComputerName $computername -Filter "name=""$service""" -Credential $credential