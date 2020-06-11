# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

function Get-Skip
{
    if ($testingLibararyUsage -or ($PSVersionTable.PSVersion -lt '5.0'))
    {
        return $true
    }
    if ($IsLinux -or $IsMacOS)
    {
        $dscIsInstalled = Test-Path /etc/opt/omi/conf/dsc/configuration
        if (-not $dscIsInstalled)
        {
            return $true
        }
    }
    return $false
}
$skipTest = Get-Skip

Describe "Resolve DSC Resource Dependency" {
    BeforeAll {
        $savedPSModulePath = $env:PSModulePath
        $violationFileName = 'MissingDSCResource.ps1'
        $violationFilePath = Join-Path $PSScriptRoot $violationFileName
        $testRootDirectory = Split-Path -Parent $PSScriptRoot
        Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

        Function Test-EnvironmentVariables($oldEnv)
        {
            $newEnv = Get-Item Env:\* | Sort-Object -Property Key
            $newEnv.Count | Should -Be $oldEnv.Count
            foreach ($index in 1..$newEnv.Count)
            {
                $newEnv[$index].Key | Should -Be $oldEnv[$index].Key
                $newEnv[$index].Value | Should -Be $oldEnv[$index].Value
            }
        }

        Function Get-LocalAppDataFolder
        {
            if ($IsLinux -or $IsMacOS) { $env:HOME } else { $env:LOCALAPPDATA }
        }
    }
    AfterAll {
        if ( $skipTest ) { return }
        $env:PSModulePath = $savedPSModulePath
    }

    Context "Module handler class" {
        BeforeAll {
            if ($PSVersionTable.PSVersion -lt '5.0') { return }
            $moduleHandlerType = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.ModuleDependencyHandler]
            $oldEnvVars = Get-Item Env:\* | Sort-Object -Property Key
            $savedPSModulePath = $env:PSModulePath
        }
        AfterAll {
            if ( $skipTest ) { return }
            $env:PSModulePath = $savedPSModulePath
        }
        It "Sets defaults correctly" -Skip:($PSVersionTable.PSVersion -lt '5.0') {
            $rsp = [runspacefactory]::CreateRunspace()
            $rsp.Open()
            $depHandler = $moduleHandlerType::new($rsp)

            $expectedPath = [System.IO.Path]::GetTempPath()
            $depHandler.TempPath | Should -Be $expectedPath

            $depHandler.LocalAppDataPath | Should -Be (Get-LocalAppDataFolder)

            $expectedModuleRepository = "PSGallery"
            $depHandler.ModuleRepository | Should -Be $expectedModuleRepository

            $expectedPssaAppDataPath = Join-Path $depHandler.LocalAppDataPath "PSScriptAnalyzer"
            $depHandler.PSSAAppDataPath | Should -Be $expectedPssaAppDataPath

            $expectedPSModulePath = $savedPSModulePath + [System.IO.Path]::PathSeparator + $depHandler.TempModulePath
            $env:PSModulePath | Should -Be $expectedPSModulePath

            $depHandler.Dispose()
            $rsp.Dispose()
        }

        It "Keeps the environment variables unchanged" -Skip:($PSVersionTable.PSVersion -lt '5.0') {
            Test-EnvironmentVariables($oldEnvVars)
        }

        It "Throws if runspace is null" -Skip:($PSVersionTable.PSVersion -lt '5.0') {
            {$moduleHandlerType::new($null)} | Should -Throw
        }

        It "Throws if runspace is not opened" -Skip:($PSVersionTable.PSVersion -lt '5.0') {
            $rsp = [runspacefactory]::CreateRunspace()
            {$moduleHandlerType::new($rsp)} | Should -Throw
            $rsp.Dispose()
        }

        It "Extracts 1 module name" -skip:$skipTest {
            $sb = @"
{Configuration SomeConfiguration
{
    Import-DscResource -ModuleName SomeDscModule1
}}
"@
            $tokens = $null
            $parseError = $null
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($sb, [ref]$tokens, [ref]$parseError)
            $resultModuleNames = $moduleHandlerType::GetModuleNameFromErrorExtent($parseError[0], $ast, [ref]$null).ToArray()
            $resultModuleNames[0] | Should -Be 'SomeDscModule1'
        }

        It "Extracts 1 module name with version" -skip:$skipTest {
            $sb = @"
{Configuration SomeConfiguration
{
    Import-DscResource -ModuleName SomeDscModule1 -ModuleVersion 1.2.3.4
}}
"@
            $tokens = $null
            $parseError = $null
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($sb, [ref]$tokens, [ref]$parseError)
            $moduleVersion = $null
            $resultModuleNames = $moduleHandlerType::GetModuleNameFromErrorExtent($parseError[0], $ast, [ref]$moduleVersion).ToArray()
            $resultModuleNames[0] | Should -Be 'SomeDscModule1'
            $moduleVersion | Should -Be ([version]'1.2.3.4')
        }

        It "Extracts 1 module name with version using HashTable syntax" -skip:$skipTest {
            $sb = @"
{Configuration SomeConfiguration
{
    Import-DscResource -ModuleName (@{ModuleName="SomeDscModule1";ModuleVersion="1.2.3.4"})
}}
"@
            $tokens = $null
            $parseError = $null
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($sb, [ref]$tokens, [ref]$parseError)
            $moduleVersion = $null
            $resultModuleNames = $moduleHandlerType::GetModuleNameFromErrorExtent($parseError[0], $ast, [ref]$moduleVersion).ToArray()
            $resultModuleNames[0] | Should -Be 'SomeDscModule1'
            $moduleVersion | Should -Be ([version]'1.2.3.4')
        }

        It "Extracts more than 1 module names" -skip:$skipTest {
            $sb = @"
{Configuration SomeConfiguration
{
    Import-DscResource -ModuleName SomeDscModule1,SomeDscModule2,SomeDscModule3
}}
"@
            $tokens = $null
            $parseError = $null
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($sb, [ref]$tokens, [ref]$parseError)
            $resultModuleNames = $moduleHandlerType::GetModuleNameFromErrorExtent($parseError[0], $ast, [ref]$null).ToArray()
            $resultModuleNames[0] | Should -Be 'SomeDscModule1'
            $resultModuleNames[1] | Should -Be 'SomeDscModule2'
            $resultModuleNames[2] | Should -Be 'SomeDscModule3'
        }


        It "Extracts module names when ModuleName parameter is not the first named parameter" -skip:$skipTest {
            $sb = @"
{Configuration SomeConfiguration
{
    Import-DscResource -Name SomeName -ModuleName SomeDscModule1
}}
"@
            $tokens = $null
            $parseError = $null
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($sb, [ref]$tokens, [ref]$parseError)
            $resultModuleNames = $moduleHandlerType::GetModuleNameFromErrorExtent($parseError[0], $ast, [ref]$null).ToArray()
            $resultModuleNames[0] | Should -Be 'SomeDscModule1'
        }
    }

    Context "Invoke-ScriptAnalyzer without switch" {
        It "Has parse errors" -skip:$skipTest {
            $dr = Invoke-ScriptAnalyzer -Path $violationFilePath -ErrorVariable analyzerErrors -ErrorAction SilentlyContinue
            $analyzerErrors.Count | Should -Be 0

            $dr |
                Where-Object { $_.Severity -eq "ParseError" } |
                Get-Count | Should -Be 1
        }
    }

    Context "Invoke-ScriptAnalyzer without switch but with module in temp path" -Skip:$skipTest {
        BeforeAll {
            if ( $skipTest ) { return }
            $oldEnvVars = Get-Item Env:\* | Sort-Object -Property Key
            $moduleName = "MyDscResource"
            $modulePath = "$(Split-Path $PSScriptRoot)\Rules\DSCResourceModule\DSCResources\$moduleName"

            # Save the current environment variables
            $oldLocalAppDataPath = Get-LocalAppDataFolder
            $oldTempPath = $env:TEMP
            $savedPSModulePath = $env:PSModulePath

            # set the environment variables
            $tempPath = Join-Path ([System.IO.Path]::GetTempPath()) ([guid]::NewGUID()).ToString()
            $newLocalAppDataPath = Join-Path $tempPath "LocalAppData"
            $newTempPath = Join-Path $tempPath "Temp"
            if (-not ($IsLinux -or $IsMacOS))
            {
                $env:LOCALAPPDATA = $newLocalAppDataPath
                $env:TEMP = $newTempPath
            }

            # create the temporary directories
            New-Item -Type Directory -Path $newLocalAppDataPath -force
            New-Item -Type Directory -Path $newTempPath -force

            # create and dispose module dependency handler object
            # to setup the temporary module
            $rsp = [runspacefactory]::CreateRunspace()
            $rsp.Open()
            $depHandler = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.ModuleDependencyHandler]::new($rsp)
            $pssaAppDataPath = $depHandler.PSSAAppDataPath
            $tempModulePath = $depHandler.TempModulePath
            $rsp.Dispose()
            $depHandler.Dispose()

            # copy myresource module to the temporary location
            # we could let the module dependency handler download it from psgallery
            Copy-Item -Recurse -Path $modulePath -Destination $tempModulePath
        }

        AfterAll {
            if ( $skipTest ) { return }
            $env:PSModulePath = $savedPSModulePath
        }

        It "has a single parse error" -skip:$skipTest {
            $dr = Invoke-ScriptAnalyzer -Path $violationFilePath -ErrorVariable analyzerErrors -ErrorAction SilentlyContinue
            $analyzerErrors.Count | Should -Be 0
            $dr |
                Where-Object { $_.Severity -eq "ParseError" } |
                Get-Count | Should -Be 1
        }

        It "Keeps PSModulePath unchanged before and after invocation" -skip:$skipTest {
            Invoke-ScriptAnalyzer -Path $violationFilePath -ErrorVariable parseErrors -ErrorAction SilentlyContinue
            $env:PSModulePath | Should -Be $savedPSModulePath
        }

        It "Keeps the environment variables unchanged" -skip:$skipTest {
            if ($IsLinux -or $IsMacOS)
            {
                $env:HOME = $oldLocalAppDataPath
                # On Linux [System.IO.Path]::GetTempPath() does not use the TEMP env variable unlike on Windows
            }
            else
            {
                $env:LOCALAPPDATA = $oldLocalAppDataPath
                $env:TEMP = $oldTempPath
            }
            Remove-Item -Recurse -Path $tempModulePath -Force
            Remove-Item -Recurse -Path $tempPath -Force

            Test-EnvironmentVariables($oldEnvVars)
        }
    }
}
