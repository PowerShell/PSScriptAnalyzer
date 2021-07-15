# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeDiscovery {
    $RunningInCIOnUbuntu = $IsLinux -and ($env:TF_BUILD -or $env:APPVEYOR) # some test cases randomly start and stop to fail in Ubuntu CI tests

    $Srv2012_3_profile = 'win-8_x64_6.2.9200.0_3.0_x64_4.0.30319.42000_framework'
    $Srv2012r2_4_profile = 'win-8_x64_6.3.9600.0_4.0_x64_4.0.30319.42000_framework'
    $Srv2016_5_profile = 'win-8_x64_10.0.14393.0_5.1.14393.2791_x64_4.0.30319.42000_framework'
    $Srv2016_6_2_profile = 'win-8_x64_10.0.14393.0_6.2.4_x64_4.0.30319.42000_core'
    $Srv2016_7_profile = 'win-8_x64_10.0.14393.0_7.0.0_x64_3.1.2_core'
    $Srv2019_5_profile = 'win-8_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework'
    $Srv2019_6_2_profile = 'win-8_x64_10.0.17763.0_6.2.4_x64_4.0.30319.42000_core'
    $Srv2019_7_profile = 'win-8_x64_10.0.17763.0_7.0.0_x64_3.1.2_core'
    $Win10_5_profile = 'win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework'
    $Win10_6_2_profile = 'win-4_x64_10.0.18362.0_6.2.4_x64_4.0.30319.42000_core'
    $Win10_7_profile = 'win-4_x64_10.0.18362.0_7.0.0_x64_3.1.2_core'
    $Ubuntu1804_6_2_profile = 'ubuntu_x64_18.04_6.2.4_x64_4.0.30319.42000_core'
    $Ubuntu1804_7_profile = 'ubuntu_x64_18.04_7.0.0_x64_3.1.2_core'

    $CompatibilityTestCases = @(
        @{ Target = $Srv2012_3_profile; Script = 'Write-Information "Information"'; Commands = @("Write-Information"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = '"Hello World" | ConvertFrom-String | Get-Member'; Commands = @("ConvertFrom-String"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Compress-Archive -LiteralPath C:\Reference\Draftdoc.docx, C:\Reference\Images\diagram2.vsd -CompressionLevel Optimal -DestinationPath C:\Archives\Draft.Zip'; Commands = @("Compress-Archive"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Get-Runspace -Id 2'; Commands = @("Get-Runspace"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = '$Protected = "Hello World" | Protect-CmsMessage -To "*youralias@emailaddress.com*"'; Commands = @("Protect-CmsMessage"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Format-Hex -Path "C:\temp\temp.t7f"'; Commands = @("Format-Hex"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Set-Clipboard -Value "This is a test string"'; Commands = @("Set-Clipboard"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Clear-RecycleBin -Force'; Commands = @("Clear-RecycleBin"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = '$TempFile = New-TemporaryFile'; Commands = @("New-TemporaryFile"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'New-Guid | Out-String'; Commands = @("New-Guid"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Enter-PSHostProcess -Name powershell_ise'; Commands = @("Enter-PSHostProcess"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Wait-Debugger'; Commands = @("Wait-Debugger"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Start-Job { Write-Host "Hello" } | Debug-Job'; Commands = @("Debug-Job"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Get-ItemPropertyValue -Path HKLM:\SOFTWARE\Microsoft\PowerShell\3\PowerShellEngine -Name ApplicationBase'; Commands = @("Get-ItemPropertyValue"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Get-FileHash $pshome\powershell.exe | Format-List'; Commands = @("Get-FileHash"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Get-ChildItem ./ | Format-List'; Commands = @(); Version = "3.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2012_3_profile; Script = 'Save-Help -Module $m -DestinationPath "C:\SavedHelp"'; Commands = @(); Version = "3.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2012_3_profile; Script = 'gci .'; Commands = @(); Version = "3.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2012_3_profile; Script = 'iex $expr | % { Transform $_ }'; Commands = @(); Version = "3.0"; OS = "Windows"; ProblemCount = 0 }

        @{ Target = $Srv2012r2_4_profile; Script = 'Write-Information "Information"'; Commands = @("Write-Information"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = '"Hello World" | ConvertFrom-String | Get-Member'; Commands = @("ConvertFrom-String"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Compress-Archive -LiteralPath C:\Reference\Draftdoc.docx, C:\Reference\Images\diagram2.vsd -CompressionLevel Optimal -DestinationPath C:\Archives\Draft.Zip'; Commands = @("Compress-Archive"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Get-Runspace -Id 2'; Commands = @("Get-Runspace"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Format-Hex -Path "C:\temp\temp.t7f"'; Commands = @("Format-Hex"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Set-Clipboard -Value "This is a test string"'; Commands = @("Set-Clipboard"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Clear-RecycleBin -Force'; Commands = @("Clear-RecycleBin"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = '$TempFile = New-TemporaryFile'; Commands = @("New-TemporaryFile"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'New-Guid | Out-String'; Commands = @("New-Guid"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Enter-PSHostProcess -Name powershell_ise'; Commands = @("Enter-PSHostProcess"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Wait-Debugger'; Commands = @("Wait-Debugger"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Start-Job { Write-Host "Hello" } | Debug-Job'; Commands = @("Debug-Job"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Get-ItemPropertyValue -Path HKLM:\SOFTWARE\Microsoft\PowerShell\3\PowerShellEngine -Name ApplicationBase'; Commands = @("Get-ItemPropertyValue"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Get-ChildItem ./ | Format-List'; Commands = @(); Version = "3.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2012r2_4_profile; Script = 'gci .'; Commands = @(); Version = "4.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2012r2_4_profile; Script = 'iex $expr | % { Transform $_ }'; Commands = @(); Version = "4.0"; OS = "Windows"; ProblemCount = 0 }

        @{ Target = $Srv2019_5_profile; Script = "Remove-Alias gcm"; Commands = @("Remove-Alias"); Version = "5.1"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_5_profile; Script = "Get-Uptime"; Commands = @("Get-Uptime"); Version = "5.1"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_5_profile; Script = "Remove-Service 'MyService'"; Commands = @("Remove-Service"); Version = "5.1"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_5_profile; Script = 'Get-ChildItem ./ | Format-List'; Commands = @(); Version = "3.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_5_profile; Script = 'gci .'; Commands = @(); Version = "5.1"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_5_profile; Script = 'iex $expr | % { Transform $_ }'; Commands = @(); Version = "5.1"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_5_profile; Script = 'fhx $filePath'; Commands = @(); Version = "5.1"; OS = "Windows"; ProblemCount = 0 }

        @{ Target = $Srv2019_6_2_profile; Script = "Add-PSSnapIn MySnapIn"; Commands = @("Add-PSSnapIn"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = "Get-PSSnapIn MySnapIn"; Commands = @("Get-PSSnapIn"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = "Get-Content $pshome\about_signing.help.txt | Out-Printer"; Commands = @("Out-Printer"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'New-PSWorkflowSession -ComputerName "ServerNode01" -Name "WorkflowTests" -SessionOption (New-PSSessionOption -OutputBufferingMode Drop)'; Commands = @("New-PSWorkflowSession"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = "Get-Process | Out-GridView"; Commands = @("Out-GridView"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = "Get-Process | ogv"; Commands = @("ogv"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = "Remove-PSSnapIn MySnapIn"; Commands = @("Remove-PSSnapIn"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = "Show-Command"; Commands = @("Show-Command"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = '$composers | Convert-String -Example "first middle last=last, first"'; Commands = @("Convert-String"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Export-Console -Path $pshome\Consoles\ConsoleS1.psc1'; Commands = @("Export-Console"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Get-Counter "\Processor(*)\% Processor Time" | Export-Counter -Path $home\Counters.blg'; Commands = @("Get-Counter", "Export-Counter"); Version = "6.2"; OS = "Windows"; ProblemCount = 2 }
        @{ Target = $Srv2019_6_2_profile; Script = 'curl $uri'; Commands = @("curl"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Get-ChildItem ./ | Format-List'; Commands = @(); Version = "3.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_6_2_profile; Script = 'gci .'; Commands = @(); Version = "6.2"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2016_6_2_profile; Script = 'iex $expr | % { Transform $_ }'; Commands = @(); Version = "6.2"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_6_2_profile; Script = 'ConvertFrom-String $str'; Commands = @("ConvertFrom-String"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = "Get-WmiObject -Class Win32_Process"; Commands = @("Get-WmiObject"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = "Invoke-WmiMethod -Path win32_process -Name create -ArgumentList notepad.exe"; Commands = @("Invoke-WmiMethod"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = '$np | Remove-WmiObject'; Commands = @("Remove-WmiObject"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Set-Clipboard -Value "This is a test string"'; Commands = @("Set-Clipboard"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = "Set-WmiInstance -Class Win32_WMISetting -Argument @{LoggingLevel=2}"; Commands = @("Set-WmiInstance"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Add-Computer -DomainName "Domain01" -Restart'; Commands = @("Add-Computer"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Checkpoint-Computer -Description "Install MyApp"'; Commands = @("Checkpoint-Computer"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Clear-EventLog "Windows PowerShell"'; Commands = @("Clear-EventLog"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Clear-RecycleBin'; Commands = @("Clear-RecycleBin"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Start-Transaction; New-Item MyCompany -UseTransaction; Complete-Transaction'; Commands = @("Start-Transaction", "Complete-Transaction"); Version = "6.2"; OS = "Windows"; ProblemCount = 2 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Disable-ComputerRestore -Drive "C:\"'; Commands = @("Disable-ComputerRestore"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Enable-ComputerRestore -Drive "C:\", "D:\"'; Commands = @("Enable-ComputerRestore"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Get-ControlPanelItem -Name "*Program*", "*App*"'; Commands = @("Get-ControlPanelItem"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Get-EventLog -Newest 5 -LogName "Application"'; Commands = @("Get-EventLog"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Get-HotFix -Description "Security*" -ComputerName "Server01", "Server02" -Cred "Server01\admin01"'; Commands = @("Get-HotFix"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = '$zip = New-WebServiceProxy -Uri "http://www.webservicex.net/uszip.asmx?WSDL"'; Commands = @("New-WebServiceProxy"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }

        @{ Target = $Srv2019_7_profile; Script = "Add-PSSnapIn MySnapIn"; Commands = @("Add-PSSnapIn"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = "Get-PSSnapIn MySnapIn"; Commands = @("Get-PSSnapIn"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = "Get-Content $pshome\about_signing.help.txt | Out-Printer"; Commands = @(); Version = "7.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_7_profile; Script = 'New-PSWorkflowSession -ComputerName "ServerNode01" -Name "WorkflowTests" -SessionOption (New-PSSessionOption -OutputBufferingMode Drop)'; Commands = @("New-PSWorkflowSession"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = "Get-Process | Out-GridView"; Commands = @(); Version = "7.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_7_profile; Script = "Get-Process | ogv"; Commands = @(); Version = "7.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_7_profile; Script = "Remove-PSSnapIn MySnapIn"; Commands = @("Remove-PSSnapIn"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = "Show-Command"; Commands = @(); Version = "7.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_7_profile; Script = '$composers | Convert-String -Example "first middle last=last, first"'; Commands = @("Convert-String"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Export-Console -Path $pshome\Consoles\ConsoleS1.psc1'; Commands = @("Export-Console"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Get-ChildItem ./ | Format-List'; Commands = @(); Version = "7.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_7_profile; Script = 'gci .'; Commands = @(); Version = "7.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2016_7_profile; Script = 'iex $expr | % { Transform $_ }'; Commands = @(); Version = "7.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_7_profile; Script = 'ConvertFrom-String $str'; Commands = @("ConvertFrom-String"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = "Get-WmiObject -Class Win32_Process"; Commands = @("Get-WmiObject"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = "Invoke-WmiMethod -Path win32_process -Name create -ArgumentList notepad.exe"; Commands = @("Invoke-WmiMethod"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = '$np | Remove-WmiObject'; Commands = @("Remove-WmiObject"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Set-Clipboard -Value "This is a test string"'; Commands = @(); Version = "7.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_7_profile; Script = "Set-WmiInstance -Class Win32_WMISetting -Argument @{LoggingLevel=2}"; Commands = @("Set-WmiInstance"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Add-Computer -DomainName "Domain01" -Restart'; Commands = @("Add-Computer"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Checkpoint-Computer -Description "Install MyApp"'; Commands = @("Checkpoint-Computer"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Clear-EventLog "Windows PowerShell"'; Commands = @("Clear-EventLog"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Clear-RecycleBin'; Commands = @(); Version = "7.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_7_profile; Script = 'Start-Transaction; New-Item MyCompany -UseTransaction; Complete-Transaction'; Commands = @("Start-Transaction", "Complete-Transaction"); Version = "7.0"; OS = "Windows"; ProblemCount = 2 }
        @{ Target = $Srv2019_7_profile; Script = 'Disable-ComputerRestore -Drive "C:\"'; Commands = @("Disable-ComputerRestore"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Enable-ComputerRestore -Drive "C:\", "D:\"'; Commands = @("Enable-ComputerRestore"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Get-ControlPanelItem -Name "*Program*", "*App*"'; Commands = @("Get-ControlPanelItem"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Get-EventLog -Newest 5 -LogName "Application"'; Commands = @("Get-EventLog"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Get-HotFix -Description "Security*" -ComputerName "Server01", "Server02" -Cred "Server01\admin01"'; Commands = @(); Version = "7.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_7_profile; Script = '$zip = New-WebServiceProxy -Uri "http://www.webservicex.net/uszip.asmx?WSDL"'; Commands = @("New-WebServiceProxy"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }

        @{ Target = $Ubuntu1804_6_2_profile; Script = 'Get-AuthenticodeSignature ./script.ps1'; Commands = @("Get-AuthenticodeSignature"); Version = "6.2"; OS = "Linux"; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_6_2_profile; Script = 'Get-Service systemd'; Commands = @("Get-Service"); Version = "6.2"; OS = "Linux"; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_6_2_profile; Script = 'Start-Service -Name "sshd"'; Commands = @("Start-Service"); Version = "6.2"; OS = "Linux"; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_6_2_profile; Script = 'Get-PSSessionConfiguration -Name Full  | Format-List -Property *'; Commands = @("Get-PSSessionConfiguration"); Version = "6.2"; OS = "Linux"; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_6_2_profile; Script = 'Get-CimInstance Win32_StartupCommand'; Commands = @("Get-CimInstance"); Version = "6.2"; OS = "Linux"; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_6_2_profile; Script = 'Get-ChildItem ./ | Format-List'; Commands = @(); Version = "6.2"; OS = "Linux"; ProblemCount = 0 }
        @{ Target = $Ubuntu1804_6_2_profile; Script = 'gci .'; Commands = @(); Version = "6.2"; OS = "Linux"; ProblemCount = 0 }
        @{ Target = $Ubuntu1804_6_2_profile; Script = 'iex $expr | % { Transform $_ }'; Commands = @(); Version = "6.2"; OS = "Linux"; ProblemCount = 0 }

        @{ Target = $Ubuntu1804_7_profile; Script = 'Get-AuthenticodeSignature ./script.ps1'; Commands = @("Get-AuthenticodeSignature"); Version = "7.0"; OS = "Linux"; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_7_profile; Script = 'Get-Service systemd'; Commands = @("Get-Service"); Version = "7.0"; OS = "Linux"; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_7_profile; Script = 'Start-Service -Name "sshd"'; Commands = @("Start-Service"); Version = "7.0"; OS = "Linux"; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_7_profile; Script = 'Get-PSSessionConfiguration -Name Full  | Format-List -Property *'; Commands = @("Get-PSSessionConfiguration"); Version = "7.0"; OS = "Linux"; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_7_profile; Script = 'Get-CimInstance Win32_StartupCommand'; Commands = @("Get-CimInstance"); Version = "7.0"; OS = "Linux"; ProblemCount = 1 }
        @{ Target = $Ubuntu1804_7_profile; Script = 'Get-ChildItem ./ | Format-List'; Commands = @(); Version = "7.0"; OS = "Linux"; ProblemCount = 0 }
        @{ Target = $Ubuntu1804_7_profile; Script = 'gci .'; Commands = @(); Version = "7.0"; OS = "Linux"; ProblemCount = 0 }
        @{ Target = $Ubuntu1804_7_profile; Script = 'iex $expr | % { Transform $_ }'; Commands = @(); Version = "7.0"; OS = "Linux"; ProblemCount = 0 }

        @{ Target = $Srv2019_6_2_profile; Script = 'ConvertFrom-String $str'; Commands = @("ConvertFrom-String"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = "Get-WmiObject -Class Win32_Process"; Commands = @("Get-WmiObject"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = "Invoke-WmiMethod -Path win32_process -Name create -ArgumentList notepad.exe"; Commands = @("Invoke-WmiMethod"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = '$np | Remove-WmiObject'; Commands = @("Remove-WmiObject"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Set-Clipboard -Value "This is a test string"'; Commands = @("Set-Clipboard"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = "Set-WmiInstance -Class Win32_WMISetting -Argument @{LoggingLevel=2}"; Commands = @("Set-WmiInstance"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Add-Computer -DomainName "Domain01" -Restart'; Commands = @("Add-Computer"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Checkpoint-Computer -Description "Install MyApp"'; Commands = @("Checkpoint-Computer"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Clear-EventLog "Windows PowerShell"'; Commands = @("Clear-EventLog"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Clear-RecycleBin'; Commands = @("Clear-RecycleBin"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Start-Transaction; New-Item MyCompany -UseTransaction; Complete-Transaction'; Commands = @("Start-Transaction", "Complete-Transaction"); Version = "6.2"; OS = "Windows"; ProblemCount = 2 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Disable-ComputerRestore -Drive "C:\"'; Commands = @("Disable-ComputerRestore"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Enable-ComputerRestore -Drive "C:\", "D:\"'; Commands = @("Enable-ComputerRestore"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Get-ControlPanelItem -Name "*Program*", "*App*"'; Commands = @("Get-ControlPanelItem"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Get-EventLog -Newest 5 -LogName "Application"'; Commands = @("Get-EventLog"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Get-HotFix -Description "Security*" -ComputerName "Server01", "Server02" -Cred "Server01\admin01"'; Commands = @("Get-HotFix"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = '$zip = New-WebServiceProxy -Uri "http://www.webservicex.net/uszip.asmx?WSDL"'; Commands = @("New-WebServiceProxy"); Version = "6.2"; OS = "Windows"; ProblemCount = 1 }

        @{ Target = $Srv2019_7_profile; Script = 'ConvertFrom-String $str'; Commands = @("ConvertFrom-String"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = "Get-WmiObject -Class Win32_Process"; Commands = @("Get-WmiObject"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = "Invoke-WmiMethod -Path win32_process -Name create -ArgumentList notepad.exe"; Commands = @("Invoke-WmiMethod"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = '$np | Remove-WmiObject'; Commands = @("Remove-WmiObject"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Set-Clipboard -Value "This is a test string"'; Commands = @(); Version = "7.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_7_profile; Script = "Set-WmiInstance -Class Win32_WMISetting -Argument @{LoggingLevel=2}"; Commands = @("Set-WmiInstance"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Add-Computer -DomainName "Domain01" -Restart'; Commands = @("Add-Computer"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Checkpoint-Computer -Description "Install MyApp"'; Commands = @("Checkpoint-Computer"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Clear-EventLog "Windows PowerShell"'; Commands = @("Clear-EventLog"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Clear-RecycleBin'; Commands = @(); Version = "7.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_7_profile; Script = 'Start-Transaction; New-Item MyCompany -UseTransaction; Complete-Transaction'; Commands = @("Start-Transaction", "Complete-Transaction"); Version = "7.0"; OS = "Windows"; ProblemCount = 2 }
        @{ Target = $Srv2019_7_profile; Script = 'Disable-ComputerRestore -Drive "C:\"'; Commands = @("Disable-ComputerRestore"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Enable-ComputerRestore -Drive "C:\", "D:\"'; Commands = @("Enable-ComputerRestore"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Get-ControlPanelItem -Name "*Program*", "*App*"'; Commands = @("Get-ControlPanelItem"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Get-EventLog -Newest 5 -LogName "Application"'; Commands = @("Get-EventLog"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
        @{ Target = $Srv2019_7_profile; Script = 'Get-HotFix -Description "Security*" -ComputerName "Server01", "Server02" -Cred "Server01\admin01"'; Commands = @(); Version = "7.0"; OS = "Windows"; ProblemCount = 0 }
        @{ Target = $Srv2019_7_profile; Script = '$zip = New-WebServiceProxy -Uri "http://www.webservicex.net/uszip.asmx?WSDL"'; Commands = @("New-WebServiceProxy"); Version = "7.0"; OS = "Windows"; ProblemCount = 1 }
    )

    $ParameterCompatibilityTestCases = @(
        @{ Target = $Srv2012_3_profile; Script = 'Get-Item -Path ./ -InformationVariable i'; Commands = @('Get-Item'); Parameters = @('InformationVariable'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Get-ChildItem -Path ./ -Recurse -Depth 3'; Commands = @('Get-ChildItem'); Parameters = @('Depth'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Copy-Item -Path C:\myFile.txt -ToSession $s -DestinationFolder d:\destinationFolder'; Commands = @('Copy-Item', 'Copy-Item'); Parameters = @('ToSession', 'DestinationFolder'); Version = '3.0'; OS = 'Windows'; ProblemCount = 2 }
        @{ Target = $Srv2012_3_profile; Script = '"File content" | Out-File ./file.txt -NoNewline'; Commands = @('Out-File'); Parameters = @('NoNewline'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Set-Content "Hi" -Path C:/path/to/thing.ps1 -NoNewline'; Commands = @('Set-Content'); Parameters = @('NoNewline'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Get-Command -ShowCommandInfo'; Commands = @('Get-Command'); Parameters = @('ShowCommandInfo'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Import-Module -FullyQualifiedName @{ ModuleName = "PSScriptAnalyzer"; ModuleVersion = "1.17" }'; Commands = @('Import-Module'); Parameters = @('FullyQualifiedName'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Remove-Module -FullyQualifiedName @{ ModuleName = "PSScriptAnalyzer"; ModuleVersion = "1.17" }'; Commands = @('Remove-Module'); Parameters = @('FullyQualifiedName'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Register-ScheduledJob -RunNow -Trigger $t'; Commands = @('Register-ScheduledJob'); Parameters = @('RunNow'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'New-JobTrigger -At "1/20/2012 3:00 AM" -RepeatIndefinitely'; Commands = @('New-JobTrigger'); Parameters = @('RepeatIndefinitely'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = '$t = Get-ScheduledJob | Get-JobTrigger | Enable-JobTrigger -PassThru'; Commands = @('Enable-JobTrigger'); Parameters = @('PassThru'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Save-Help -FullyQualifiedModule @{ ModuleName = "MyModule"; MaximumVersion = "2.7" }'; Commands = @('Save-Help'); Parameters = @('FullyQualifiedModule'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Export-PSSession -FullyQualifiedModule @{ ModuleName = "MyModule"; RequiredVersion = $reqVer }'; Commands = @('Export-PSSession'); Parameters = @('FullyQualifiedModule'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Get-Command -FullyQualifiedModule @{ ModuleName = $m; MaximumVersion = $maxVer }'; Commands = @('Get-Command'); Parameters = @('FullyQualifiedModule'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Get-Module -FullyQualifiedName @{ ModuleName = $m; ModuleVersion = $v }'; Commands = @('Get-Module'); Parameters = @('FullyQualifiedName'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Get-Process -PipelineVariable proc | ForEach-Object { Format-Table $Proc }'; Commands = @('Get-Process'); Parameters = @('PipelineVariable'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012_3_profile; Script = 'Get-Process -IncludeUserName'; Commands = @('Get-Process'); Parameters = @('IncludeUserName'); Version = '3.0'; OS = 'Windows'; ProblemCount = 1 }

        @{ Target = $Srv2012r2_4_profile; Script = 'Get-Item -Path ./ -InformationVariable i'; Commands = @('Get-Item'); Parameters = @('InformationVariable'); Version = '4.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Get-ChildItem -Path ./ -Recurse -Depth 3'; Commands = @('Get-ChildItem'); Parameters = @('Depth'); Version = '4.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Copy-Item -Path C:\myFile.txt -ToSession $s -DestinationFolder d:\destinationFolder'; Commands = @('Copy-Item', 'Copy-Item'); Parameters = @('ToSession', 'DestinationFolder'); Version = '4.0'; OS = 'Windows'; ProblemCount = 2 }
        @{ Target = $Srv2012r2_4_profile; Script = '"File content" | Out-File ./file.txt -NoNewline'; Commands = @('Out-File'); Parameters = @('NoNewline'); Version = '4.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Set-Content "Hi" -Path C:/path/to/thing.ps1 -NoNewline'; Commands = @('Set-Content'); Parameters = @('NoNewline'); Version = '4.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Get-Command -ShowCommandInfo'; Commands = @('Get-Command'); Parameters = @('ShowCommandInfo'); Version = '4.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Import-Module -FullyQualifiedName @{ ModuleName = "PSScriptAnalyzer"; ModuleVersion = "1.17" }'; Commands = @('Import-Module'); Parameters = @('FullyQualifiedName'); Version = '4.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Remove-Module -FullyQualifiedName @{ ModuleName = "PSScriptAnalyzer"; ModuleVersion = "1.17" }'; Commands = @('Remove-Module'); Parameters = @('FullyQualifiedName'); Version = '4.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Save-Help -FullyQualifiedModule @{ ModuleName = "MyModule"; MaximumVersion = "2.7" }'; Commands = @('Save-Help'); Parameters = @('FullyQualifiedModule'); Version = '4.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Export-PSSession -FullyQualifiedModule @{ ModuleName = "MyModule"; MaximumVersion = "2.7" }'; Commands = @('Export-PSSession'); Parameters = @('FullyQualifiedModule'); Version = '4.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Get-Command -FullyQualifiedModule @{ ModuleName = "MyModule"; MaximumVersion = "2.7" }'; Commands = @('Get-Command'); Parameters = @('FullyQualifiedModule'); Version = '4.0'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Register-ScheduledJob -RunNow -Trigger $t'; Commands = @('Register-ScheduledJob'); Parameters = @('RunNow'); Version = '4.0'; OS = 'Windows'; ProblemCount = 0 }
        @{ Target = $Srv2012r2_4_profile; Script = 'Get-Module -FullyQualifiedName @{ ModuleName = $m; ModuleVersion = $v }'; Commands = @('Get-Module'); Parameters = @('FullyQualifiedName'); Version = '4.0'; OS = 'Windows'; ProblemCount = 0 }

        @{ Target = $Srv2019_5_profile; Script = 'Invoke-RestMethod "https://example.com" -FollowRelLink -MaximumFollowRelLink 10 -CustomMethod "Squash"'; Commands = @('Invoke-RestMethod', 'Invoke-RestMethod', 'Invoke-RestMethod'); Parameters = @('FollowRelLink', 'MaximumFollowRelLink', 'CustomMethod'); Version = '5.1'; OS = 'Windows'; ProblemCount = 3 }
        @{ Target = $Srv2019_5_profile; Script = 'Invoke-WebRequest "https://microsoft.com" -NoProxy -ContentType "something-strange" -SkipHeaderValidation'; Commands = @('Invoke-WebRequest', 'Invoke-WebRequest'); Parameters = @('NoProxy', 'SkipHeaderValidation'); Version = '5.1'; OS = 'Windows'; ProblemCount = 2 }
        @{ Target = $Srv2019_5_profile; Script = 'Invoke-RestMethod "https://mywebsite.com/auth" -Authentication OAuth -Token $tok -ResponseHeadersVariable resp'; Commands = @('Invoke-RestMethod', 'Invoke-RestMethod', 'Invoke-RestMethod'); Parameters = @('Authentication', 'Token', 'ResponseHeadersVariable'); Version = '5.1'; OS = 'Windows'; ProblemCount = 3 }
        @{ Target = $Srv2019_5_profile; Script = '$obj = ConvertFrom-Json $json -AsHashtable'; Commands = @('ConvertFrom-Json'); Parameters = @('AsHashtable'); Version = '5.1'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2019_5_profile; Script = 'Get-ChildItem . -FollowSymLink'; Commands = @('Get-ChildItem'); Parameters = @('FollowSymLink'); Version = '5.1'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2019_5_profile; Script = 'Import-Module "MyModule" -ErrorAction SilentlyContinue'; Commands = @(); Parameters = @(); Version = '5.1'; OS = 'Windows'; ProblemCount = 0 }
        @{ Target = $Srv2019_5_profile; Script = 'Get-Location | Split-Path -LeafBase'; Commands = @('Split-Path'); Parameters = @('LeafBase'); Version = '5.1'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2019_5_profile; Script = 'Get-Process | sort PagedMemorySize -Top 10'; Commands = @('sort'); Parameters = @('Top'); Version = '5.1'; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2019_5_profile; Script = 'Get-Process | select -first 1 | Out-String -NoNewline'; Commands = @('Out-String'); Parameters = @('NoNewline'); Version = '5.1'; OS = 'Windows'; ProblemCount = 1 }

        @{ Target = $Srv2019_6_2_profile; Script = 'Get-Help "Invoke-WebRequest" -ShowWindow'; Commands = @('Get-Help'); Parameters = @('ShowWindow'); Version = "6.2"; OS = 'Windows'; ProblemCount = 1 }
        @{ Target = $Srv2019_6_2_profile; Script = 'Start-Service "eventlog" -ComputerName "MyComputer"'; Commands = @('Start-Service'); Parameters = @('ComputerName'); Version = "6.2"; OS = 'Windows'; ProblemCount = 1 }

        @{ Target = $Srv2019_7_profile; Script = 'Get-Help "Invoke-WebRequest" -ShowWindow'; Commands = @(); Parameters = @(); Version = "7.0"; OS = 'Windows'; ProblemCount = 0 }
        @{ Target = $Srv2019_7_profile; Script = 'Start-Service "eventlog" -ComputerName "MyComputer"'; Commands = @('Start-Service'); Parameters = @('ComputerName'); Version = "7.0"; OS = 'Windows'; ProblemCount = 1 }
    )
}

Describe 'UseCompatibleCommands' {
    BeforeAll {
        $RuleName = 'PSUseCompatibleCommands'

        $Srv2012_3_profile = 'win-8_x64_6.2.9200.0_3.0_x64_4.0.30319.42000_framework'
        $Srv2012r2_4_profile = 'win-8_x64_6.3.9600.0_4.0_x64_4.0.30319.42000_framework'
        $Srv2016_5_profile = 'win-8_x64_10.0.14393.0_5.1.14393.2791_x64_4.0.30319.42000_framework'
        $Srv2016_6_2_profile = 'win-8_x64_10.0.14393.0_6.2.4_x64_4.0.30319.42000_core'
        $Srv2016_7_profile = 'win-8_x64_10.0.14393.0_7.0.0_x64_3.1.2_core'
        $Srv2019_5_profile = 'win-8_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework'
        $Srv2019_6_2_profile = 'win-8_x64_10.0.17763.0_6.2.4_x64_4.0.30319.42000_core'
        $Srv2019_7_profile = 'win-8_x64_10.0.17763.0_7.0.0_x64_3.1.2_core'
        $Win10_5_profile = 'win-48_x64_10.0.17763.0_5.1.17763.316_x64_4.0.30319.42000_framework'
        $Win10_6_2_profile = 'win-4_x64_10.0.18362.0_6.2.4_x64_4.0.30319.42000_core'
        $Win10_7_profile = 'win-4_x64_10.0.18362.0_7.0.0_x64_3.1.2_core'
        $Ubuntu1804_6_2_profile = 'ubuntu_x64_18.04_6.2.4_x64_4.0.30319.42000_core'
        $Ubuntu1804_7_profile = 'ubuntu_x64_18.04_7.0.0_x64_3.1.2_core'

        $AzF_profile = (Resolve-Path "$PSScriptRoot/../../PSCompatibilityCollector/optional_profiles/azurefunctions.json").Path
        $AzA_profile = (Resolve-Path "$PSScriptRoot/../../PSCompatibilityCollector/optional_profiles/azureautomation.json").Path

        $TargetProfileConfigKey = 'TargetProfiles'

    }

    Context 'Targeting a single profile' {
        It "Reports <ProblemCount> command incompatibilties with '<Script>' on <OS> with PowerShell <Version>" -TestCases $CompatibilityTestCases -Skip:$RunningInCIOnUbuntu {
            param($Script, [string]$Target, [string[]]$Commands, [version]$Version, [string]$OS, [int]$ProblemCount)

            $settings = @{
                Rules = @{
                    $RuleName = @{
                        Enable = $true
                        $TargetProfileConfigKey = @($Target)
                    }
                }
            }

            $diagnostics = Invoke-ScriptAnalyzer -IncludeRule $RuleName -ScriptDefinition $Script -Settings $settings `
                | Where-Object { -not $_.Parameter } # Filter out diagnostics about incompatible parameters

            $diagnostics.Count | Should -Be $ProblemCount -Because ($diagnostics.RuleName -join ', ')

            for ($i = 0; $i -lt $diagnostics.Count; $i++)
            {
                $diagnostics[$i].Command | Should -BeExactly $Commands[$i]
                $diagnostics[$i].TargetPlatform.OperatingSystem.Family | Should -Be $OS
                $diagnostics[$i].TargetPlatform.PowerShell.Version.Major | Should -Be $Version.Major
                $diagnostics[$i].TargetPlatform.PowerShell.Version.Minor | Should -Be $Version.Minor
            }
        }

        It "Reports <ProblemCount> parameter incompatibilties for '<Parameters>' on '<Commands>' with '<Script>' on <OS> with PowerShell <Version>" -TestCases $ParameterCompatibilityTestCases -Skip:$RunningInCIOnUbuntu {
            param($Script, [string]$Target, [string[]]$Commands, [string[]]$Parameters, [version]$Version, [string]$OS, [int]$ProblemCount)

            $settings = @{
                Rules = @{
                    $RuleName = @{
                        Enable = $true
                        $TargetProfileConfigKey = @($Target)
                    }
                }
            }

            $diagnostics = Invoke-ScriptAnalyzer -IncludeRule $RuleName -ScriptDefinition $Script -Settings $settings `
                | Where-Object { $_.Parameter } # Filter out diagnostics about incompatible parameters

            $diagnostics.Count | Should -Be $ProblemCount -Because ($diagnostics.RuleName -join ', ')

            for ($i = 0; $i -lt $diagnostics.Count; $i++)
            {
                $diagnostics[$i].Command | Should -BeExactly $Commands[$i]
                $diagnostics[$i].TargetPlatform.OperatingSystem.Family | Should -Be $OS
                $diagnostics[$i].TargetPlatform.PowerShell.Version.Major | Should -Be $Version.Major
                $diagnostics[$i].TargetPlatform.PowerShell.Version.Minor | Should -Be $Version.Minor
            }
        }
    }

    Context "Checking a file against many targets" {
        It "Finds all command problems" -Skip:$RunningInCIOnUbuntu {
            $settings = @{
                Rules = @{
                    $RuleName = @{
                        Enable = $true
                        $TargetProfileConfigKey = @(
                            $Srv2012_3_profile
                            $Srv2012r2_4_profile
                            $Srv2019_5_profile
                            $Srv2019_6_2_profile
                            $Srv2019_7_profile
                            $Ubuntu1804_6_2_profile
                            $Ubuntu1804_7_profile
                        )
                    }
                }
            }

            $diagnostics = Invoke-ScriptAnalyzer -Path "$PSScriptRoot/CompatibilityRuleAssets/IncompatibleScript.ps1" -IncludeRule $RuleName -Settings $settings -Severity Information,Warning,Error

            # Classes cause diagnostics to be emitted in PS 3 and 4
            $expectedNumber = 17

            $diagnostics.Count | Should -Be $expectedNumber -Because "Got diagnostics: $($diagnostics.Message -join ", ")"

            $diagnosticGroups = Group-Object -InputObject $diagnostics -Property Command

            foreach ($group in $diagnosticGroups)
            {
                switch ($group.Name)
                {
                    'Get-EventLog'
                    {
                        $group.Count | Should -Be 7
                        break
                    }

                    'Import-Module'
                    {
                        $group.Count | Should -Be 2
                        break
                    }

                    'Invoke-WebRequest'
                    {
                        $group.Count | Should -Be 10
                        break
                    }

                    'ogv'
                    {
                        $group.Count | Should -Be 7
                        break
                    }

                    'Write-Information'
                    {
                        $group.Count | Should -Be 2
                        break
                    }
                }
            }
        }
    }

    Context "Checking PSSA repository scripts" {
        It "Ensures that PSSA build scripts are cross compatible with everything" {
            $settings = @{
                Rules = @{
                    $RuleName = @{
                        Enable = $true
                        $TargetProfileConfigKey = @(
                            $Srv2012_3_profile
                            $Srv2012r2_4_profile
                            $Srv2016_5_profile
                            $Srv2016_6_2_profile
                            $Srv2019_5_profile
                            $Srv2019_6_2_profile
                            $Srv2019_7_profile
                            $Win10_5_profile
                            $Win10_6_2_profile
                            $Win10_7_profile
                            $Ubuntu1804_6_2_profile
                            $Ubuntu1804_7_profile
                        )
                        IgnoreCommands = @(
                            'Install-Module'
                            # Some PowerShell profiles have Pester installed by default
                            # So Pester is legitimately flagged
                            'Describe'
                            'It'
                            'Should'
                            'Be'
                            'BeforeAll'
                            'Context'
                            'AfterAll'
                            'BeforeEach'
                            'AfterEach'
                            'Mock'
                            'Assert-MockCalled'
                        )
                    }
                }
            }

            $diagnostics = Invoke-ScriptAnalyzer -Path "$PSScriptRoot/../../" -IncludeRule $RuleName -Settings $settings
            $diagnostics.Count | Should -Be 0 -Because ($diagnostics.Message -join ', ')
        }
    }

    Context 'Targeting new-form Az profiles alongside older profiles' {
        BeforeAll {
            $settings = @{
                Rules = @{
                    $RuleName = @{
                        Enable = $true
                        $TargetProfileConfigKey = @(
                            $AzF_profile
                            $AzA_profile
                            $Win10_5_profile
                        )
                    }
                }
            }
        }

        It "Finds AzF problems with a script" {
            $diagnostics = Invoke-ScriptAnalyzer -IncludeRule $RuleName -Settings $settings -ScriptDefinition '
                Get-WmiObject Win32_Process
                New-SelfSignedCertificate
                Invoke-MySpecialFunction
            '

            $diagnostics.Count | Should -Be 2 -Because ($diagnostics.RuleName -join ', ')
            $diagnosticGroups = Group-Object -InputObject $diagnostics -Property Command
            foreach ($group in $diagnosticGroups)
            {
                switch ($group.Name)
                {
                    'Get-WmiObject'
                    {
                        $group.Count | Should -Be 1
                        $group.Group[0].Command | Should -BeExactly 'Get-WmiObject'
                        break
                    }

                    'New-SelfSignedCertificate'
                    {
                        $group.Count | Should -Be 1
                        $group.Group[0].Command | Should -BeExactly 'New-SelfSignedCertificate'
                        break
                    }
                }
            }
        }
    }
}
