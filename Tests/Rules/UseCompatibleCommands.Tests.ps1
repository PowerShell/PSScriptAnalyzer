Import-Module ([System.IO.Path]::Combine($PSScriptRoot, '..', '..', 'out', 'PSScriptAnalyzer'))

$script:RuleName = 'UseCompatibleCommands'
$script:AnyProfileConfigKey = 'AnyProfilePath'
$script:TargetProfileConfigKey = 'TargetProfilePaths'
$script:AssetDirPath = Join-Path $PSScriptRoot $script:RuleName

$script:Srv2012_3_profile = 'win-8_x64_6.2.9200.0_3.0_x64_4.0.30319.42000_framework'
$script:Srv2012r2_4_profile = 'win-8_x64_6.3.9600.0_4.0_x64_4.0.30319.42000_framework'
$script:Srv2019_5_profile = 'win-8_x64_10.0.17763.0_5.1.17763.134_x64_4.0.30319.42000_framework'
$script:Srv2019_6_1_profile = 'win-8_x64_10.0.17763.0_6.1.2_x64_4.0.30319.42000_core'
$script:Ubuntu1804_6_1_profile = 'ubuntu_x64_18.04_6.1.2_x64_4.0.30319.42000_core'

$script:CompatibilityTestCases = @(
    @{ Target = $script:Srv2012_3_profile; Script = 'Write-Information "Information"'; Commands = @("Write-Information"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = '"Hello World" | ConvertFrom-String | Get-Member'; Commands = @("ConvertFrom-String"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = 'Compress-Archive -LiteralPath C:\Reference\Draftdoc.docx, C:\Reference\Images\diagram2.vsd -CompressionLevel Optimal -DestinationPath C:\Archives\Draft.Zip'; Commands = @("Compress-Archive"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = 'Get-Runspace -Id 2'; Commands = @("Get-Runspace"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = '$Protected = "Hello World" | Protect-CmsMessage -To "*youralias@emailaddress.com*"'; Commands = @("Protect-CmsMessage"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = 'Format-Hex -Path "C:\temp\temp.t7f"'; Commands = @("Format-Hex"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = 'Set-Clipboard -Value "This is a test string"'; Commands = @("Set-Clipboard"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = 'Clear-RecycleBin -Force'; Commands = @("Clear-RecycleBin"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = '$TempFile = New-TemporaryFile'; Commands = @("New-TemporaryFile"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = 'New-Guid | Out-String'; Commands = @("New-Guid"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = 'Enter-PSHostProcess -Name powershell_ise'; Commands = @("Enter-PSHostProcess"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = 'Wait-Debugger'; Commands = @("Wait-Debugger"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = 'Start-Job { Write-Host "Hello" } | Debug-Job'; Commands = @("Debug-Job"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = 'Get-ItemPropertyValue -Path HKLM:\SOFTWARE\Microsoft\PowerShell\3\PowerShellEngine -Name ApplicationBase'; Commands = @("Get-ItemPropertyValue"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012_3_profile; Script = 'Get-FileHash $pshome\powershell.exe | Format-List'; Commands = @("Get-FileHash"); Version = "3.0"; OS = "Windows"; ProblemCount = 1 }

    @{ Target = $script:Srv2012r2_4_profile; Script = 'Write-Information "Information"'; Commands = @("Write-Information"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012r2_4_profile; Script = '"Hello World" | ConvertFrom-String | Get-Member'; Commands = @("ConvertFrom-String"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012r2_4_profile; Script = 'Compress-Archive -LiteralPath C:\Reference\Draftdoc.docx, C:\Reference\Images\diagram2.vsd -CompressionLevel Optimal -DestinationPath C:\Archives\Draft.Zip'; Commands = @("Compress-Archive"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012r2_4_profile; Script = 'Get-Runspace -Id 2'; Commands = @("Get-Runspace"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012r2_4_profile; Script = 'Format-Hex -Path "C:\temp\temp.t7f"'; Commands = @("Format-Hex"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012r2_4_profile; Script = 'Set-Clipboard -Value "This is a test string"'; Commands = @("Set-Clipboard"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012r2_4_profile; Script = 'Clear-RecycleBin -Force'; Commands = @("Clear-RecycleBin"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012r2_4_profile; Script = '$TempFile = New-TemporaryFile'; Commands = @("New-TemporaryFile"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012r2_4_profile; Script = 'New-Guid | Out-String'; Commands = @("New-Guid"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012r2_4_profile; Script = 'Enter-PSHostProcess -Name powershell_ise'; Commands = @("Enter-PSHostProcess"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012r2_4_profile; Script = 'Wait-Debugger'; Commands = @("Wait-Debugger"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012r2_4_profile; Script = 'Start-Job { Write-Host "Hello" } | Debug-Job'; Commands = @("Debug-Job"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2012r2_4_profile; Script = 'Get-ItemPropertyValue -Path HKLM:\SOFTWARE\Microsoft\PowerShell\3\PowerShellEngine -Name ApplicationBase'; Commands = @("Get-ItemPropertyValue"); Version = "4.0"; OS = "Windows"; ProblemCount = 1 }

    @{ Target = $script:Srv2019_5_profile; Script = "Remove-Alias gcm"; Commands = @("Remove-Alias"); Version = "5.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_5_profile; Script = "Get-Uptime"; Commands = @("Get-Uptime"); Version = "5.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_5_profile; Script = "Remove-Service 'MyService'"; Commands = @("Remove-Service"); Version = "5.1"; OS = "Windows"; ProblemCount = 1 }

    @{ Target = $script:Srv2019_6_1_profile; Script = "Add-PSSnapIn MySnapIn"; Commands = @("Add-PSSnapIn"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'ConvertFrom-String $str'; Commands = @("ConvertFrom-String"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = '$cb = Get-Clipboard'; Commands = @("Get-Clipboard"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = "Get-PSSnapIn MySnapIn"; Commands = @("Get-PSSnapIn"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = "Get-WmiObject -Class Win32_Process"; Commands = @("Get-WmiObject"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = "Invoke-WmiMethod -Path win32_process -Name create -ArgumentList notepad.exe"; Commands = @("Invoke-WmiMethod"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = "Get-Content $pshome\about_signing.help.txt | Out-Printer"; Commands = @("Out-Printer"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'New-PSWorkflowSession -ComputerName "ServerNode01" -Name "WorkflowTests" -SessionOption (New-PSSessionOption -OutputBufferingMode Drop)'; Commands = @("New-PSWorkflowSession"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = "Get-Process | Out-GridView"; Commands = @("Out-GridView"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = "Remove-PSSnapIn MySnapIn"; Commands = @("Remove-PSSnapIn"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = '$np | Remove-WmiObject'; Commands = @("Remove-WmiObject"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'Set-Clipboard -Value "This is a test string"'; Commands = @("Set-Clipboard"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = "Show-Command"; Commands = @("Show-Command"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = "Set-WmiInstance -Class Win32_WMISetting -Argument @{LoggingLevel=2}"; Commands = @("Set-WmiInstance"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'Add-Computer -DomainName "Domain01" -Restart'; Commands = @("Add-Computer"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'Checkpoint-Computer -Description "Install MyApp"'; Commands = @("Checkpoint-Computer"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'Clear-EventLog "Windows PowerShell"'; Commands = @("Clear-EventLog"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'Clear-RecycleBin'; Commands = @("Clear-RecycleBin"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'Start-Transaction; New-Item MyCompany -UseTransaction; Complete-Transaction'; Commands = @("Start-Transaction", "Complete-Transaction"); Version = "6.1"; OS = "Windows"; ProblemCount = 2 }
    @{ Target = $script:Srv2019_6_1_profile; Script = '$composers | Convert-String -Example "first middle last=last, first"'; Commands = @("Convert-String"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'Disable-ComputerRestore -Drive "C:\"'; Commands = @("Disable-ComputerRestore"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'Enable-ComputerRestore -Drive "C:\", "D:\"'; Commands = @("Enable-ComputerRestore"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'Export-Console -Path $pshome\Consoles\ConsoleS1.psc1'; Commands = @("Export-Console"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'Get-Counter "\Processor(*)\% Processor Time" | Export-Counter -Path $home\Counters.blg'; Commands = @("Get-Counter", "Export-Counter"); Version = "6.1"; OS = "Windows"; ProblemCount = 2 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'Get-ControlPanelItem -Name "*Program*", "*App*"'; Commands = @("Get-ControlPanelItem"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'Get-EventLog -Newest 5 -LogName "Application"'; Commands = @("Get-EventLog"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'Get-HotFix -Description "Security*" -ComputerName "Server01", "Server02" -Cred "Server01\admin01"'; Commands = @("Get-HotFix"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = '$zip = New-WebServiceProxy -Uri "http://www.webservicex.net/uszip.asmx?WSDL"'; Commands = @("New-WebServiceProxy"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = $script:Srv2019_6_1_profile; Script = 'curl $uri'; Commands = @("curl"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }

    @{ Target = $script:Ubuntu1804_6_1_profile; Script = 'Get-AuthenticodeSignature ./script.ps1'; Commands = @("Get-AuthenticodeSignature"); Version = "6.1"; OS = "Linux"; ProblemCount = 1 }
    @{ Target = $script:Ubuntu1804_6_1_profile; Script = 'Get-Service systemd'; Commands = @("Get-Service"); Version = "6.1"; OS = "Linux"; ProblemCount = 1 }
    @{ Target = $script:Ubuntu1804_6_1_profile; Script = 'Start-Service -Name "sshd"'; Commands = @("Start-Service"); Version = "6.1"; OS = "Linux"; ProblemCount = 1 }
    @{ Target = $script:Ubuntu1804_6_1_profile; Script = 'Get-PSSessionConfiguration -Name Full  | Format-List -Property *'; Commands = @("Get-PSSessionConfiguration"); Version = "6.1"; OS = "Linux"; ProblemCount = 1 }
    @{ Target = $script:Ubuntu1804_6_1_profile; Script = 'Get-CimInstance Win32_StartupCommand'; Commands = @("Get-CimInstance"); Version = "6.1"; OS = "Linux"; ProblemCount = 1 }
)

Describe 'UseCompatibleCommands' {
    Context 'Targeting a single profile' {
        It "Reports <ProblemCount> problem(s) with <Script> on <OS> with PowerShell <Version>, targeting <Target>" -TestCases $script:CompatibilityTestCases {
            param($Script, [string]$Target, [string[]]$Commands, [version]$Version, [string]$OS, [int]$ProblemCount)

            $settings = @{
                Rules = @{
                    $script:RuleName = @{
                        Enable = $true
                        TargetProfilePaths = @($Target)
                    }
                }
            }

            $diagnostics = Invoke-ScriptAnalyzer -IncludeRule $script:RuleName -ScriptDefinition $Script -Settings $settings

            $diagnostics.Count | Should -Be $ProblemCount

            for ($i = 0; $i -lt $diagnostics.Count; $i++)
            {
                $diagnostics[$i].Command | Should -BeExactly $Commands[$i]
                $diagnostics[$i].TargetPlatform.OperatingSystem.Family | Should -Be $OS
                $diagnostics[$i].TargetPlatform.PowerShell.Version.Major | Should -Be $Version.Major
                $diagnostics[$i].TargetPlatform.PowerShell.Version.Minor | Should -Be $Version.Minor
            }
        }
    }
}