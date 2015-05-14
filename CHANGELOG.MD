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

