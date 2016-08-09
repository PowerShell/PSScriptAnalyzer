#AvoidUsingDeprecatedManifestFields	
**Severity Level: Warning**

##Description
In PowerShell 5.0, a number of fields in module manifest files (.psd1) have been changed.

The field ```ModuleToProcess``` has been replaced with the ```RootModule``` field.

##How to Fix
Replace ```ModuleToProcess``` with ```RootModule``` in the module manifest.

##Example
###Wrong： 
``` PowerShell
ModuleToProcess ='psscriptanalyzer'

ModuleVersion = '1.0'
```

###Correct: 
``` PowerShell
RootModule ='psscriptanalyzer'

ModuleVersion = '1.0'
```
