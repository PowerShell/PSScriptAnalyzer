@echo off
setlocal
call "%VS120COMNTOOLS%\VsDevCmd.bat"
msbuild .\PSScriptAnalyzer.sln
if NOT [%ERRORLEVEL%]==[0] pause
endlocal