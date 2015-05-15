@echo off
setlocal
if "%VS120COMNTOOLS%"=="" GOTO NOTOOLS
call "%VS120COMNTOOLS%\VsDevCmd.bat"
msbuild .\PSScriptAnalyzer.sln  /p:Configuration=Debug /l:FileLogger,Microsoft.Build.Engine;logfile=PSScriptAnalyzer_Build.log;append=true
if NOT [%ERRORLEVEL%]==[0] pause

GOTO END

:NOTOOLS
echo The Visual Studio 2013 tools are not installed
pause

:END
endlocal