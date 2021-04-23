Invoke-Command -ComputerName $comp
Invoke-Command -ComputerName $env:COMPUTERNAME
Invoke-Command -ComputerName localhost
Invoke-Command -ComputerName .
Invoke-Command -ComputerName ::1
Invoke-Command -ComputerName 127.0.0.1
Invoke-Command -ComputerName:'localhost'
Invoke-Command -ComputerName:"."
Invoke-Command -ComputerName:'::1'
Invoke-Command -ComputerName:"127.0.0.1"
Invoke-Command -ComputerName "localhost"
Invoke-Command -ComputerName "."
Invoke-Command -ComputerName "::1"
Invoke-Command -ComputerName "127.0.0.1"