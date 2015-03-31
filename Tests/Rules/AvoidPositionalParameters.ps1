Get-Content Test
Get-ChildItem Tests
Write-Output "I don't want to use positional parameters"
Split-Path "RandomPath" -Leaf
Get-Process | ForEach-Object {Write-Host $_.name -foregroundcolor cyan}