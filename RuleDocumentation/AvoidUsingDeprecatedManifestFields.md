#AvoidUsingDeprecatedManifestFields	
**Severity Level: Warning**


##Description

PowerShell V5.0 introduced some new fields and replaced some old fields with in module manifest files (.psd1). Therefore, fields such as "ModuleToProcess" is replaced with "RootModule". Using the deprecated manifest fields will result in PSScriptAnalyzer warnings.

##How to Fix

To fix a violation of this, please replace "ModuleToProcess" with "RootModule".

##Example

Wrong： 
```
ModuleToProcess ='psscriptanalyzer'

ModuleVersion = '1.0'
```

Correct: 
```
RootModule ='psscriptanalyzer'

ModuleVersion = '1.0'
```
