##PSScriptAnalyzer For NanoServer
We have created a few rules to help check script compliance on NanoServer. To be able to run PSScriptAnalyzer NanoServer version, there are a few extra steps needed to set up the PSScriptAnalyzer module.

###Requirements
You will need to download extra assemblies [System.Reflection.Metadata](https://www.nuget.org/packages/System.Reflection.Metadata/) from NuGet.

Follow the instructions on NuGet to install the assemblies into your forked PSScriptAnalyzer NanoServer local repository.

###Run NanoServer Rules
PSScriptAnalyzer still works the same way when you invoke rules. 
To make PSScriptAnalyzer only run NanoServer rules, you can choose to add -IncludeRule parameter to Invoke-ScriptAnalyzer, eg:

Invoke-ScriptAnalyzer -IncludeRule *Nano