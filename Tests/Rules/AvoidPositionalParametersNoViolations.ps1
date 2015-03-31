Write-Output -InputObject "Use positional parameters plz!"
Get-ChildItem -Path "Tests"
Clear-Host
Split-Path -Path "Random" -leaf
Get-Process | Where-Object {$_.handles -gt 200}
get-service-computername localhost | where {($_.status -eq "Running") -and ($_.CanStop -eq $true)}