## Released v1.1.1 (Nov.3, 2015)
###Features:
- Support for PSDrives when using Invoke-ScriptAnalyzer
- More robust Profiles feature - better defaulting when supplied with invalid profile - actionable Warnings
- Validated integration with ISESteroids, ISEScriptAnalyzerAddon & VS Code Editor Services Plugin

###Rules:
- New rule to ensure that a file with missing BOM is encoded in ASCII

###Fixes:
- Doc Updates, Cleaned up source files to reflect accurate comments
- Fix SuppressedOnly switch functionality
- Updated Positional parameter rule to trigger only when used with > 3 positional parameters

##

## Released v1.1.0 (Sep.1, 2015)
###Features:
- Support for using ScriptAnalyzer as a .net library - ScriptAnalyzer APIs
- Support for ScriptAnalyzer Profiles
- Documentation for using Inline Rule Suppression
- Added about help topic file as part of the module

###Rules:
- Rule to checks for UTF8 encoding in help file
- Deprecate Uninitialized Variable rule as per community feedback


###Fixes:
- Fix false positive for UsingInternalURL
- WriteVerbose only when analyzing valid powershell files
- DSCClass rules not being applied when exclude rule is used
- Add host to list of initialized variable
- Exclude external non-powershell applications (Console/GUI) from Positional Parameter rule application
- Additional heuristics for detecting psavoidusingplaintextforpassword rule violation

##

## Released v1.0.2 (June.24, 2015)
###Features:
- Perf improvements in the Engine to execute rules concurrently.


###Rules:
- New rule to validate the presence of deprecated module manifest fields.
- Removed PSAvoidTrapStatement rule from the builtin set – since this is not deprecated and using trap is a better choice in certain scenarios.


###Fixes:
- Verbose Message rule applies to only DSC cmdlet based resources.
- Multiple fixes to AvoidUninitializedVariable to work with non-mandatory parameters, fix in the flow graphs for throw statements;  support for missing preference variables; support for automatic variables.
- PSAvoidUsingInternalsURLs to work with xPath expressions.
- UseSingularNouns rule to raise warnings for plural phrases.
- Added .gitignore to exclude certain files from being reported as untracked.
- Revisited severity for DSC rules.
- PSUseOutputTypeCorrectly rule not to get triggered for functions returning system.void type.
- PSAvoidDefaultTrueValueSwitchParameter to work with switch attribute when supplied as fully qualified.
- Ignore PSObject and PSCustomObject for UseOutputTypeCorrectly rule.
- Only raise NullComparisonRule if the RHS is an array or has unknown type.
- PSUseDeclaredVarsMoreThanAssignments rule to be raised for script variables and for setting the property of a variable.
- Support for using PSUseCmdletCorrectly rule when mandatory parameters are supplied in a splatted hashtable.
- AvoidUsingPlainTextForPassword rule to be raised only strings or object types.
- Fix for PositionalParameterUsed method (Helper.cs) uses unsafe method to exclude ForEach-Object and Where-Object.

##

## Released v1.0.1 (May.8, 2015)
###Features:
- Integrated with waffle.io for Project Management.
- Added documentation for writing script rules.

###Rules:
- AvoidUsingWMICmdlet rule: For PowerShell 3.0 and above, usage of WMI cmdlets is not recommended. This rule is to detect WMI cmdlet usage in scripts that are written for PS 3.0 and above.
- DSCTestsPresent rule: Resource module contains Tests folder with tests for given resource.
- UseOutputTypeCorrectly rule: If we can identify the type of an object that is outputted to the pipeline by a cmdlet, then that type must be listed in the OutputType attribute.

###Fixes:

- PSProvideVerboseMessage only throws warnings in non-advanced functions.
- Fix the issue in importing customized rule
- Fix Function Member Ast cast error

##

##Released v1.0.0 on Apr.24, 2015
###Features:
- Finalized three levels of Severity - Error/Warning/Information. 
- Improved PSScriptAnalyzer engine behavior: emits non-terminating errors (Ex: for failed ast parse) and continues rule application when running on multiple scripts.
- Added wild card supports for rules in Invoke-ScriptAnalyzer and Get-ScriptAnalyzer. Eg. Invoke-ScriptAnalyzer -IncludeRule PSAvoid* will apply all rules starting with PSAvoid* in built in rule assemblies. 
- Added -Severity to Get-ScriptAnalyzerRules. Get-ScriptAnalyzer -Severity will filter rules based on the severity given.
- Added Suppression functionality. Users are now able to specify suppression on certain parts of the scripts by specifying "SupressMessageAttribute" in the scripts. More details and documentations will be coming soon in blog posts. Also comes with this feature is the ability for users to display a list of suppressed messages.

###Rules:

- Added DSC Rules for resources including Parameter validation, Usage of standard DSC functions and return type validation. Rule checkings also support for DSC classes. Built-in DSC rules include:
    + UseStandardDSCFunctionsInResource
    + UseIdenticalParametersDSC
    + UseIdenticalMandatoryParametersDSC
    + ReturnCorrectTypesForDSCFunctions
- Added support in the engine to detect DSC configuration/resource files and disable default rule checkings on DSC configuration and resource files.
- UseShouldProcessForStateChangingFunctions - If an advanced function has Verbs like New/Start/Stop/Restart/Reset/Set- that will change system state, it should support ShouldProcess attribute.


###Fixes:

- Improved heuristics to detect usage of Username and Password instead of PSCredential type.
- Improved accuracy in the detection of uninitialized variables.
- Improved error messages to include error line numbers and file names.
- Identified usage of PSBound parameters and PowerShell supplied variables such as $MyInvocation to avoid unnecessary noise in the results returned by some of the built-in rules.
- Fixed terminating errors including "Illegal characters in Path".

