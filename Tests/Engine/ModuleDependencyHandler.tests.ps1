if (!(Get-Module PSScriptAnalyzer) -and !$testingLibraryUsage)
{
	Import-Module PSScriptAnalyzer
}

if ($testingLibraryUsage)
{
    return
}

$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
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
        $oldEnvVars = Get-Item Env:\* | Sort-Object -Property Key
        $oldPSModulePath = $env:PSModulePath
        It "Sets defaults correctly" {
            $depHandler = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.ModuleDependencyHandler]::new()

            $expectedPath = [System.IO.Path]::GetTempPath()
            $depHandler.TempPath | Should Be $expectedPath

            $expectedLocalAppDataPath = [System.Environment]::GetEnvironmentVariable("LOCALAPPDATA");
            $depHandler.LocalAppDataPath | Should Be $expectedLocalAppDataPath

            $expectedModuleRepository = "PSGallery"
            $depHandler.ModuleRepository | Should Be $expectedModuleRepository

            $expectedPssaAppDataPath = Join-Path $depHandler.LocalAppDataPath "PSScriptAnalyzer"
            $depHandler.PSSAAppDataPath | Should Be $expectedPssaAppDataPath

            $expectedPSModulePath = $oldPSModulePath + [System.IO.Path]::PathSeparator + $depHandler.TempModulePath
            $env:PSModulePath | Should Be $expectedPSModulePath

            $depHandler.Dispose()
        }

        It "Keeps the environment variables unchanged" {
            Test-EnvironmentVariables($oldEnvVars)
        }
    }

    Context "Invoke-ScriptAnalyzer without switch" {
        It "Has parse errors" {
            $dr = Invoke-ScriptAnalyzer -Path $violationFilePath -ErrorVariable parseErrors -ErrorAction SilentlyContinue
            $parseErrors.Count | Should Be 1
        }
    }

    Context "Invoke-ScriptAnalyzer with switch" {
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
        # to setup the temporary module location
        $depHandler = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.ModuleDependencyHandler]::new()
        $pssaAppDataPath = $depHandler.PSSAAppDataPath
        $tempModulePath = $depHandler.TempModulePath
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