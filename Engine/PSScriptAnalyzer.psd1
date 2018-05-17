#
# Module manifest for module 'PSScriptAnalyzer'
#

@{

# Author of this module
Author = 'Microsoft Corporation'

# Script module or binary module file associated with this manifest.
RootModule = 'PSScriptAnalyzer.psm1'

# Version number of this module.
ModuleVersion = '1.17.0'

# ID used to uniquely identify this module
GUID = 'd6245802-193d-4068-a631-8863a4342a18'

# Company or vendor of this module
CompanyName = 'Microsoft Corporation'

# Copyright statement for this module
Copyright = '(c) Microsoft Corporation 2016. All rights reserved.'

# Description of the functionality provided by this module
Description = 'PSScriptAnalyzer provides script analysis and checks for potential code defects in the scripts by applying a group of built-in or customized rules on the scripts being analyzed.'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '3.0'

# Name of the Windows PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the Windows PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module
# DotNetFrameworkVersion = ''

# Minimum version of the common language runtime (CLR) required by this module
# CLRVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
# RequiredAssemblies = @()

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
TypesToProcess = @('ScriptAnalyzer.types.ps1xml')

# Format files (.ps1xml) to be loaded when importing this module
FormatsToProcess = @('ScriptAnalyzer.format.ps1xml')

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
# NestedModules = @()

# Functions to export from this module
FunctionsToExport = @()

# Cmdlets to export from this module
CmdletsToExport = @('Get-ScriptAnalyzerRule', 'Invoke-ScriptAnalyzer', 'Invoke-Formatter')

# Variables to export from this module
VariablesToExport = @()

# Aliases to export from this module
AliasesToExport = @()

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
# FileList = @()

# Private data to pass to the module specified in RootModule/ModuleToProcess
PrivateData = @{
    PSData = @{
        Tags = 'lint', 'bestpractice'
        LicenseUri = 'https://github.com/PowerShell/PSScriptAnalyzer/blob/master/LICENSE'
        ProjectUri = 'https://github.com/PowerShell/PSScriptAnalyzer'
        IconUri = ''
        ReleaseNotes = @'
### New Parameters

- Add `-ReportSummary` switch (#895) (Thanks @StingyJack! for the base work that got finalized by @bergmeister)
- Add `-EnableExit` switch to Invoke-ScriptAnalyzer for exit and return exit code for CI purposes (#842) (by @bergmeister)
- Add `-Fix` switch to `-Path` parameter set of `Invoke-ScriptAnalyzer` (#817, #852) (by @bergmeister)

### New Rules and Warnings

- Warn when 'Get-' prefix was omitted in `AvoidAlias` rule. (#927) (by @bergmeister)
- `AvoidAssignmentToAutomaticVariable`. NB: Currently only warns against read-only automatic variables (#864, #917) (by @bergmeister)
- `PossibleIncorrectUsageOfRedirectionOperator` and `PossibleIncorrectUsageOfAssignmentOperator`. (#859, #881) (by @bergmeister)
- Add PSAvoidTrailingWhitespace rule (#820) (Thanks @dlwyatt!)

### Fixes and Improvements

- AvoidDefaultValueForMandatoryParameter triggers when the field has specification: Mandatory=value and value!=0 (#969) (by @kalgiz)
- Do not trigger UseDeclaredVarsMoreThanAssignment for variables being used via Get-Variable (#925) (by @bergmeister)
- Make UseDeclaredVarsMoreThanAssignments not flag drive qualified variables (#958) (by @bergmeister)
- Fix PSUseDeclaredVarsMoreThanAssignments to not give false positives when using += operator (#935) (by @bergmeister)
- Tweak UseConsistentWhiteSpace formatting rule to exclude first unary operator when being used in argument (#949) (by @bergmeister)
- Allow -Setting parameter to resolve setting presets as well when object is still a PSObject in BeginProcessing (#928) (by @bergmeister)
- Add macos detection to New-CommandDataFile (#947) (Thanks @GavinEke!)
- Fix PlaceOpenBrace rule correction to take comment at the end of line into account (#929) (by @bergmeister)
- Do not trigger UseShouldProcessForStateChangingFunctions rule for workflows (#923) (by @bergmeister)
- Fix parsing the -Settings object as a path when the path object originates from an expression (#915) (by @bergmeister)
- Allow relative settings path (#909) (by @bergmeister)
- Fix AvoidDefaultValueForMandatoryParameter documentation, rule and tests (#907) (by @bergmeister)
- Fix NullReferenceException in AlignAssignmentStatement rule when CheckHashtable is enabled (#838) (by @bergmeister)
- Fix FixPSUseDeclaredVarsMoreThanAssignments to also detect variables that are strongly typed (#837) (by @bergmeister)
- Fix PSUseDeclaredVarsMoreThanAssignments when variable is assigned more than once to still give a warning (#836) (by @bergmeister)

### Engine, Building and Testing

- Allow TypeNotFound parser errors (#957) (by @bergmeister)
- Scripts needed to build and sign PSSA via MS VSTS so it can be published in the gallery (#983) (by @JamesWTruher)
- Move common test code into AppVeyor module (#961) (by @bergmeister)
- Remove extraneous import-module commands in tests (#962) (by @JamesWTruher)
- Upgrade 'System.Automation.Management' NuGet package of version 6.0.0-alpha13 to  version 6.0.2 from powershell-core feed, which requires upgrade to netstandard2.0. NB: This highly improved behavior on WMF3 but also means that the latest patched version (6.0.2) of PowerShell Core should be used. (#919) by @bergmeister)
- Add Ubuntu Build+Test to Appveyor CI (#940) (by @bergmeister)
- Add PowerShell Core Build+Test to Appveyor CI (#939) (by @bergmeister)
- Update Newtonsoft.Json NuGet package of Rules project from 9.0.1 to 10.0.3 (#937) (by @bergmeister)
- Fix Pester v4 installation for `Visual Studio 2017` image and use Pester v4 assertion operator syntax (#892) (by @bergmeister)
- Have a single point of reference for the .Net Core SDK version (#885) (by @bergmeister)
- Fix regressions introduced by PR 882 (#891) (by @bergmeister)
- Changes to allow tests to be run outside of CI (#882) (by @JamesWTruher)
- Upgrade platyPS from Version 0.5 to 0.9 (#869) (by @bergmeister)
- Build using .Net Core SDK 2.1.101 targeting `netstandard2.0` and `net451` (#853, #854, #870, #899, #912, #936) (by @bergmeister)
- Add instructions to make a release (#843) (by @kapilmb)

### Documentation, Error Messages and miscellaneous Improvements

- Added Chocolatey Install help, which has community support (#999) (Thanks @pauby)
- Finalize Release Logs and bump version to 1.17 (#998) (by @bergmeister)
- Docker examples: (#987, #990) (by @bergmeister)
- Use multiple GitHub issue templates for bugs, feature requests and support questions (#986) (by @bergmeister
- Fix table of contents (#980) (by @bergmeister)
- Improve documentation, especially about parameter usage and the settings file (#968) (by @bergmeister)
- Add base changelog for 1.17.0 (#967) (by @bergmeister)
- Remove outdated about_scriptanalyzer help file (#951) (by @bergmeister)
- Fixes a typo and enhances the documentation for the parameters required for script rules (#942) (Thanks @MWL88!)
- Remove unused using statements and sort them (#931) (by @bergmeister)
- Make licence headers consistent across all .cs files by using the recommended header of PsCore (#930) (by @bergmeister)
- Update syntax in ReadMe to be the correct one from get-help (#932) by @bergmeister)
- Remove redundant, out of date Readme of RuleDocumentation folder (#918) (by @bergmeister)
- Shorten contribution section in ReadMe and make it more friendly (#911) (by @bergmeister)
- Update from Pester 4.1.1 to 4.3.1 and use new -BeTrue and -BeFalse operators (#906) (by @bergmeister)
- Fix Markdown in ScriptRuleDocumentation.md so it renders correctly on GitHub web site (#898) (Thanks @MWL88!)
- Fix typo in .Description for Measure-RequiresModules (#888) (Thanks @TimCurwick!)
- Use https links where possible (#873) (by @bergmeister)
- Make documentation of AvoidUsingPositionalParameters match the implementation (#867) (by @bergmeister)
- Fix PSAvoidUsingCmdletAliases warnings of internal build/release scripts in root and Utils folder (#872) (by @bergmeister)
- Add simple GitHub Pull Request template based off the one for PowerShell Core (#866) (by @bergmeister)
- Add a simple GitHub issue template based on the one of PowerShell Core. (#865, #884) (by @bergmeister)
- Fix Example 7 in Invoke-ScriptAnalyzer.md (#862) (Thanks @sethvs!)
- Use the typewriter apostrophe instead the typographic apostrophe (#855) (Thanks @alexandear!) 
- Add justification to ReadMe (#848) (Thanks @KevinMarquette!)
- Fix typo in README (#845) (Thanks @misterGF!)
'@
    }
}

# HelpInfo URI of this module
# HelpInfoURI = ''

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}















