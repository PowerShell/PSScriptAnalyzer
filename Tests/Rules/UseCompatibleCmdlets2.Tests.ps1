$script:AnyProfileConfigKey = 'AnyProfilePath'
$script:TargetProfileConfigKey = 'TargetProfilePaths'
$script:AssetDirPath = Join-Path $PSScriptRoot 'UseCompatibleCmdlets2'
$script:ProfileDirPath = [System.IO.Path]::Combine($PSScriptRoot, '..', '..', 'CrossCompatibility', 'profiles')
$script:AnyProfilePath = Join-Path $script:ProfileDirPath 'anyplatforms_union.json'

$script:CompatibilityTestCases = @(
    @{ Target = 'win-4_x64_10.0.18312.0_5.1.18312.1000_x64'; Script = "Remove-Alias gcm"; Commands = @("Remove-Alias"); Version = "5.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_5.1.18312.1000_x64'; Script = "Get-Uptime"; Commands = @("Get-Uptime"); Version = "5.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_5.1.18312.1000_x64'; Script = "Remove-Service 'MyService'"; Commands = @("Remove-Service"); Version = "5.1"; OS = "Windows"; ProblemCount = 1 }

    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = "Add-PSSnapIn MySnapIn"; Commands = @("Add-PSSnapIn"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'ConvertFrom-String $str'; Commands = @("ConvertFrom-String"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = '$cb = Get-Clipboard'; Commands = @("Get-Clipboard"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = "Get-PSSnapIn MySnapIn"; Commands = @("Get-PSSnapIn"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = "Get-WmiObject -Class Win32_Process"; Commands = @("Get-WmiObject"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = "Invoke-WmiMethod -Path win32_process -Name create -ArgumentList notepad.exe"; Commands = @("Invoke-WmiMethod"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = "Get-Content $pshome\about_signing.help.txt | Out-Printer"; Commands = @("Out-Printer"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'New-PSWorkflowSession -ComputerName "ServerNode01" -Name "WorkflowTests" -SessionOption (New-PSSessionOption -OutputBufferingMode Drop)'; Commands = @("New-PSWorkflowSession"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = "Get-Process | Out-GridView"; Commands = @("Out-GridView"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = "Remove-PSSnapIn MySnapIn"; Commands = @("Remove-PSSnapIn"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = '$np | Remove-WmiObject'; Commands = @("Remove-WmiObject"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'Set-Clipboard -Value "This is a test string"'; Commands = @("Set-Clipboard"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = "Show-Command"; Commands = @("Show-Command"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = "Set-WmiInstance -Class Win32_WMISetting -Argument @{LoggingLevel=2}"; Commands = @("Set-WmiInstance"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'Add-Computer -DomainName "Domain01" -Restart'; Commands = @("Add-Computer"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'Checkpoint-Computer -Description "Install MyApp"'; Commands = @("Checkpoint-Computer"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'Clear-EventLog "Windows PowerShell"'; Commands = @("Clear-EventLog"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'Clear-RecycleBin'; Commands = @("Clear-RecycleBin"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'Start-Transaction; New-Item MyCompany -UseTransaction; Complete-Transaction'; Commands = @("Start-Transaction", "Complete-Transaction"); Version = "6.1"; OS = "Windows"; ProblemCount = 2 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = '$composers | Convert-String -Example "first middle last=last, first"'; Commands = @("Convert-String"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'Disable-ComputerRestore -Drive "C:\"'; Commands = @("Disable-ComputerRestore"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'Enable-ComputerRestore -Drive "C:\", "D:\"'; Commands = @("Enable-ComputerRestore"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'Export-Console -Path $pshome\Consoles\ConsoleS1.psc1'; Commands = @("Export-Console"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'Get-Counter "\Processor(*)\% Processor Time" | Export-Counter -Path $home\Counters.blg'; Commands = @("Get-Counter", "Export-Counter"); Version = "6.1"; OS = "Windows"; ProblemCount = 2 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'Get-ControlPanelItem -Name "*Program*", "*App*"'; Commands = @("Get-ControlPanelItem"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'Get-EventLog -Newest 5 -LogName "Application"'; Commands = @("Get-EventLog"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'Get-HotFix -Description "Security*" -ComputerName "Server01", "Server02" -Cred "Server01\admin01"'; Commands = @("Get-HotFix"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = '$zip = New-WebServiceProxy -Uri "http://www.webservicex.net/uszip.asmx?WSDL"'; Commands = @("New-WebServiceProxy"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
    @{ Target = 'win-4_x64_10.0.18312.0_6.1.1_x64'; Script = 'curl $uri'; Commands = @("curl"); Version = "6.1"; OS = "Windows"; ProblemCount = 1 }
)

Describe 'UseCompatibleCmdlets2' {
    Context 'Targeting all profiles' {
        BeforeAll {
            Import-Module ([System.IO.Path]::Combine($PSScriptRoot, '..', '..', 'out', 'PSScriptAnalyzer'))
        }

        It "Reports <ProblemCount> problem(s) with <Script> on <OS> with PowerShell <Version>, targeting <Target>" -TestCases $script:CompatibilityTestCases {
            param($Script, [string]$Target, [string[]]$Commands, [version]$Version, [string]$OS, [int]$ProblemCount)

            $settings = @{
                Rules = @{
                    UseCompatibleCmdlets2 = @{
                        Enable = $true
                        TargetProfilePaths = @($Target)
                    }
                }
            }

            $diagnostics = Invoke-ScriptAnalyzer -IncludeRule 'UseCompatibleCmdlets2' -ScriptDefinition $Script -Settings $settings

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