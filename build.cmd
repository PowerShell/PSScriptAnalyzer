@echo off
setlocal
if "%VS120COMNTOOLS%"=="" GOTO NOTOOLS
call "%VS120COMNTOOLS%\VsDevCmd.bat"
set solutionPath=%1
set configuration=%2
set target=%3
if "%target%" == "clean" GOTO CLEAN
msbuild %solutionPath%  /p:Configuration=%configuration% /l:FileLogger,Microsoft.Build.Engine;logfile=PSScriptAnalyzer_Build.log;append=true
GOTO END

:NOTOOLS
echo The Visual Studio 2013 tools are not installed
GOTO END

:CLEAN
msbuild .\PSScriptAnalyzer.sln  /p:Configuration=%configuration% /t:clean /l:FileLogger,Microsoft.Build.Engine;logfile=PSScriptAnalyzer_Build.log;append=true

:END
endlocal