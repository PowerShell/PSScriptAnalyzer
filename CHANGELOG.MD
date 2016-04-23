## Release v1.5.0 (Mar. 30, 2016)

#### Engine
- Fixed an engine bug that prevented versioned script rule modules from being loaded
- Fixed loading issues with custom rules that do not have comment-based help to describe the rule
- Fixed a hang issue that appeared when using ScriptAnalyzer engine recursively with a large data set
- Housekeeping: Fixed Appveyor config to use VS 2015 and WMF 5.0 RTM image
- Community Fix: Updated the Initialize API to process the `-Settings` (formerly `-Profile`) parameter
- Perf: Improved rule filtering based on severity before invoking the engine to create tasks (threads)

#### Rules
- Fixed `UseToExportFieldsInManifest` rule to improve perf and functionality
- Fixed `AvoidNullOrEmptyHelpMessageAttribute` to use parsed values instead of ast extent
- Fixed inconsistencies in severities of rules
- Community Fix: Fixed false positives on `PSUseApprovedVerbs` when scope is declared as a prefix to the Verb-Noun combination
- Updated messages of `AvoidUsernameAndPasswordParams` rule and `UsePSCredentialType` rule

## Release v1.4.0 (Feb.16, 2016)
###Features:
- IncludeRule and ExcludeRule now consume RuleInfo objects

###Rules:
- Rule to validate HelpMessage parameter attribute value
- Rule to suggest module manifest *ToExport field values for optimizing module loading

###Fixes:
####Engine:
- Fixed bug in engine handling of severity for custom rules - this property was being ignored
- Exclude help files from being Ast parsed

####Rules:
- Emit accurate ast extents for rules - helps VSCode-PowerShell to mark violations better
- Improved heuristics for Singular noun rule - reduce false positives
- Updated credential rules to be less noisy based on community feedback
- Support for [switch] type along with [boolean] for ShouldContinueWithoutForce rule
- Improved handling of deprecated module manifest fields when PSv2.0 is specified in the manifest

## Release v1.3.0 (Jan.19, 2016)
###Features:
- Support for running ScriptAnalyzer on PowerShell version v3 or higher! This means PSv5 is no longer the minimum PS version for ScriptAnalyzer

###Rules:
- [From Community] Rule for warning about backticks that look like line-continuations but are not [MisleadingBacktick rule]
- Ability in default ruleset to recognize PowerShell variable scopes - global, local, script, private
- Ability to use functions as a variable and support for special type of functions like prompt

###Fixes:
####Rules:
- Fix for recognizing PowerShell preference variable ($OFS) in the default ruleset
- Fix for false positive - PSShouldProcess rule requires boolean value
- Fix to account for function scope prefix
- Raise ReservedParam rule only for exported functions as cmdlets

## Release v1.2.0 (Dec.17, 2015)
###Features:
- Support for consuming PowerShell content as streams (-ScriptDefinition)
- ScriptAnalyzer accepts configuration (settings) in the form of a hashtable (-Settings), added sample Settings
- Ability to run default ruleset along with custom ones in the same invocation (-IncludeDefaultRules)
- Recurse Custom Rule Paths (-RecurseCustomRulePath)
- Consistent Engine error handling when working with Settings, Default and Custom Rules

###Rules:
- Rule to detect the presence of default value for Mandatory parameters (AvoidDefaultValueForMandatoryParameter)

###Fixes:
####Engine:
- Engine update to prevent script based injection attacks
- CustomizedRulePath is now called CustomRulePath – Fixes to handle folder paths
- Fixes for RecurseCustomRulePath functionality
- Fix to binplace cmdlet help file as part of build process
- ScriptAnalyzer Profile is now called Settings
- Fix to emit filename in the diagnosticrecord when using Script based custom rules
- Fix to prevent Engine from calling Update-Help for script based custom rules
- Added additional pester tests to take care of test holes in Custom Rule feature
- Post-build error handling improvements, fixed typos in the project

####Rules:
- Fixed bug in Positional parameter rule to trigger only when used with >= 3 positional parameters
- Updated keywords that trigger PSAvoidUsingPlainTextForPassword rule
- Updated ProvideDefaultParameterValue rule to AvoidDefaultValueForMandatoryParameter rule
- Deprecate Internal Url rule based on community feedback, identified additional rules to handle hardcoded paths etc
- Added localhost exceptions for HardCodedComputerName Rule
- Update to Credential based rules to validate the presence of CredentialAttribute and PSCredential type
 
###Documentation:
- Rule & Cmdlet documentation updates – Cmdlet help file addition


##

## Release v1.1.1 (Nov.3, 2015)
###Features:
- Support for PSDrives when using Invoke-ScriptAnalyzer
- More robust Profiles feature - better defaulting when supplied with invalid profile - actionable Warnings
- Validated integration with ISESteroids, ISEScriptAnalyzerAddon

###Rules:
- New rule to ensure that a file with missing BOM is encoded in ASCII

###Fixes:
- Doc Updates, Cleaned up source files to reflect accurate comments
- Fix SuppressedOnly switch functionality
- Updated Positional parameter rule to trigger only when used with > 3 positional parameters

##

## Release v1.1.0 (Sep.1, 2015)
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

## Release v1.0.2 (June.24, 2015)
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

## Release v1.0.1 (May.8, 2015)
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

