#UseToExportFieldsInManifest 
**Severity Level: Warning**


##Description

In a module manifest, AliasesToExport, CmdletsToExport, FunctionsToExport and VariablesToExport fields should not use wildcards or $null in their entries. During module auto-discovery, if any of these entries are missing or $null or wildcard, PowerShell does some potentially expensive work to analyze the rest of the module.

##How to Fix

Please consider using an explicit list. 

##Example 1

Wrong： 
 	FunctionsToExport = $null

Correct: 
	FunctionToExport = @()

##Example 2
Suppose there are only two functions in your module, Get-Foo and Set-Foo that you want to export. Then,

Wrong:	
	FunctionsToExport = '*'

Correct:
	FunctionToExport = @(Get-Foo, Set-Foo)	