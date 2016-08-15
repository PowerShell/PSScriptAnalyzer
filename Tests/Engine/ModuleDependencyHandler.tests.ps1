if (!(Get-Module PSScriptAnalyzer) -and !$testingLibraryUsage)
{
	Import-Module PSScriptAnalyzer
}

if ($testingLibraryUsage)
{
    return
}

$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory
Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

# needs fixing
# right now we do not support module dependency handing on Linux
if ((Test-PSEditionCoreCLRLinux))
{
    return
}

# DSC Module saving is not supported in versions less than PSv5
if (($PSVersionTable.PSVersion -lt [Version]'5.0.0'))
{
    return
}

$violationFileName = 'MissingDSCResource.ps1'
$violationFilePath = Join-Path $directory $violationFileName

Describe "Resolve DSC Resource Dependency" {

    Function Test-EnvironmentVariables($oldEnv)
    {
        $newEnv = Get-Item Env:\* | Sort-Object -Property Key
        $newEnv.Count | Should Be $oldEnv.Count
        foreach ($index in 1..$newEnv.Count)
        {
            $newEnv[$index].Key | Should Be $oldEnv[$index].Key
            $newEnv[$index].Value | Should Be $oldEnv[$index].Value
        }
    }

    Context "Module handler class" {
        $moduleHandlerType = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.ModuleDependencyHandler]
        $oldEnvVars = Get-Item Env:\* | Sort-Object -Property Key
        $oldPSModulePath = $env:PSModulePath
        It "Sets defaults correctly" {
            $rsp = [runspacefactory]::CreateRunspace()
            $rsp.Open()
            $depHandler = $moduleHandlerType::new($rsp)

            $expectedPath = [System.IO.Path]::GetTempPath()
            $depHandler.TempPath | Should Be $expectedPath

            $expectedLocalAppDataPath = $env:LOCALAPPDATA
            $depHandler.LocalAppDataPath | Should Be $expectedLocalAppDataPath

            $expectedModuleRepository = "PSGallery"
            $depHandler.ModuleRepository | Should Be $expectedModuleRepository

            $expectedPssaAppDataPath = Join-Path $depHandler.LocalAppDataPath "PSScriptAnalyzer"
            $depHandler.PSSAAppDataPath | Should Be $expectedPssaAppDataPath

            $expectedPSModulePath = $oldPSModulePath + [System.IO.Path]::PathSeparator + $depHandler.TempModulePath
            $env:PSModulePath | Should Be $expectedPSModulePath

            $depHandler.Dispose()
            $rsp.Dispose()
        }

        It "Keeps the environment variables unchanged" {
            Test-EnvironmentVariables($oldEnvVars)
        }

        It "Throws if runspace is null" {
            {$moduleHandlerType::new($null)} | Should Throw
        }

        It "Throws if runspace is not opened" {
            $rsp = [runspacefactory]::CreateRunspace()
            {$moduleHandlerType::new($rsp)} | Should Throw
            $rsp.Dispose()
        }

        It "Extracts 1 module name" {
            $sb = @"
{Configuration SomeConfiguration
{
    Import-DscResource -ModuleName SomeDscModule1
}}
"@
            $tokens = $null
            $parseError = $null
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($sb, [ref]$tokens, [ref]$parseError)
            $resultModuleNames = $moduleHandlerType::GetModuleNameFromErrorExtent($parseError[0], $ast).ToArray()
            $resultModuleNames[0] | Should Be 'SomeDscModule1'
        }

        It "Extracts more than 1 module names" {
            $sb = @"
{Configuration SomeConfiguration
{
    Import-DscResource -ModuleName SomeDscModule1,SomeDscModule2,SomeDscModule3
}}
"@
            $tokens = $null
            $parseError = $null
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($sb, [ref]$tokens, [ref]$parseError)
            $resultModuleNames = $moduleHandlerType::GetModuleNameFromErrorExtent($parseError[0], $ast).ToArray()
            $resultModuleNames[0] | Should Be 'SomeDscModule1'
            $resultModuleNames[1] | Should Be 'SomeDscModule2'
            $resultModuleNames[2] | Should Be 'SomeDscModule3'
        }


        It "Extracts module names when ModuleName parameter is not the first named parameter" {
            $sb = @"
{Configuration SomeConfiguration
{
    Import-DscResource -Name SomeName -ModuleName SomeDscModule1
}}
"@
            $tokens = $null
            $parseError = $null
            $ast = [System.Management.Automation.Language.Parser]::ParseInput($sb, [ref]$tokens, [ref]$parseError)
            $resultModuleNames = $moduleHandlerType::GetModuleNameFromErrorExtent($parseError[0], $ast).ToArray()
            $resultModuleNames[0] | Should Be 'SomeDscModule1'
        }
    }

    Context "Invoke-ScriptAnalyzer without switch" {
        It "Has parse errors" {
            $dr = Invoke-ScriptAnalyzer -Path $violationFilePath -ErrorVariable parseErrors -ErrorAction SilentlyContinue
            $parseErrors.Count | Should Be 1
        }
    }

    Context "Invoke-ScriptAnalyzer without switch but with module in temp path" {
        $oldEnvVars = Get-Item Env:\* | Sort-Object -Property Key
        $moduleName = "MyDscResource"
        $modulePath = Join-Path (Join-Path (Join-Path (Split-Path $directory) "Rules") "DSCResources") $moduleName
                # Save the current environment variables
        $oldLocalAppDataPath = $env:LOCALAPPDATA
        $oldTempPath = $env:TEMP
        $oldPSModulePath = $env:PSModulePath

        # set the environment variables
        $tempPath = Join-Path $oldTempPath ([guid]::NewGUID()).ToString()
        $newLocalAppDataPath = Join-Path $tempPath "LocalAppData"
        $newTempPath = Join-Path $tempPath "Temp"
        $env:LOCALAPPDATA = $newLocalAppDataPath
        $env:TEMP = $newTempPath

        # create the temporary directories
        New-Item -Type Directory -Path $newLocalAppDataPath
        New-Item -Type Directory -Path $newTempPath

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

        It "Doesn't have parse errors" {
            # invoke script analyzer
            $dr = Invoke-ScriptAnalyzer -Path $violationFilePath -ErrorVariable parseErrors -ErrorAction SilentlyContinue
            $dr.Count | Should Be 0
        }

        It "Keeps PSModulePath unchanged before and after invocation" {
            $dr = Invoke-ScriptAnalyzer -Path $violationFilePath -ErrorVariable parseErrors -ErrorAction SilentlyContinue
            $env:PSModulePath | Should Be $oldPSModulePath
        }
        #restore environment variables and clean up temporary location
        $env:LOCALAPPDATA = $oldLocalAppDataPath
        $env:TEMP = $oldTempPath
        Remove-Item -Recurse -Path $tempModulePath -Force
        Remove-Item -Recurse -Path $tempPath -Force

        It "Keeps the environment variables unchanged" {
            Test-EnvironmentVariables($oldEnvVars)
        }
    }
}