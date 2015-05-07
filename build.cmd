@echo off
setlocal
if "%VS120COMNTOOLS%"=="" GOTO NOTOOLS
call "%VS120COMNTOOLS%\VsDevCmd.bat"
msbuild .\PSScriptAnalyzer.sln
if NOT [%ERRORLEVEL%]==[0] pause

GOTO END

:NOTOOLS
echo The Visual Studio 2012 tools are not installed
pause

:END
endlocal