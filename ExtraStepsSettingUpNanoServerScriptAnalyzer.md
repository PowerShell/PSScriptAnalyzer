##PSScriptAnalyzer For NanoServer
We have created a few rules to help check script compliance on NanoServer. To be able to run PSScriptAnalyzer NanoServer version, there are a few extra steps needed to set up the PSScriptAnalyzer module.

###Requirements
```
1. You will need to download extra assemblies [System.Reflection.Metadata](https://www.nuget.org/packages/System.Reflection.Metadata/) from NuGet.

Follow the instructions on NuGet to install the assemblies into your forked PSScriptAnalyzer NanoServer local repository. After that, you will find "packages.config" in your Engine folder as well as a new /packages folder with the installed binaries under the project folder.
```

2. Open the solution file and compile the solution. After successfully compiling the project, you will have to copy the .dlls from Reflection.Metadata (System.Collections.Immutable.dll and System.Reflection.Metadata.dll) to the binplaced PSScriptAnalyzer folder. Otherwise, the module will not be imported properly.

3. You will need a complete set of CoreCLR reference assemblies and make sure to place the folder of these .metadata_dlls under Engine folder within PSScriptAnalyzer project.

4. To import the module, go to the binplaced PSScriptAnalyzer folder and import module from there.

###Run NanoServer Rules
PSScriptAnalyzer still works the same way when you invoke rules. 
To make PSScriptAnalyzer only run NanoServer rules, you can choose to add -IncludeRule parameter to Invoke-ScriptAnalyzer, eg:

Invoke-ScriptAnalyzer -IncludeRule *Nano