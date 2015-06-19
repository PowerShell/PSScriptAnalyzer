@echo off
echo "Installing modules from %~dp0"
xcopy "%~dp0PSScriptAnalyzer" "%userprofile%\Documents\WindowsPowerShell\Modules\PSScriptAnalyzer" /y /s /i /d 

