## [1.17.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.17.0) - 2018-05-14

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

- Support `SuggestedCorrections` property on DiagnosticRecord for script based rules #1000 (by @bergmeister)
- Add CommandData files of PowerShell Core 6.0.2 for Windows/Linux/macOS and WMF3/4 that are used by UseCompatibleCmdlets rule (#954) (by @bergmeister)
- If no path is found or when using the -ScriptDefinition parameter set, default to the current location for the directory search of the implicit settings file (#979) (by @bergmeister)
- Allow TypeNotFound parser errors (#957) (by @bergmeister)
- Fix release script by building also for v3 and misc. improvements (#996) (by @bergmeister)
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
- Finalize Release Logs and bump version to 1.17 (#1002) (by @bergmeister)
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

## [1.16.1](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.16.1) - 2017-09-01
### Fixed
- (#815) Formatter crashes due to invalid extent comparisons

## [1.16.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.16.0) - 2017-08-15
### Added
- (#803) `CustomRulePath`, `RecurseCustomRulePath` and `IncludeDefaultRules` parameters to settings file.

### Fixed
- (#801) Reading DSC classes in `PSUseIdenticalMandatoryParametersForDSC` rule.
- (#796) `PSAvoidUsingWriteHost` rule documentation (Thanks @bergmeister!).

## [1.15.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.15.0) - 2017-06-21

### Added
- (#780) `Range` parameter to the `Invoke-Formatter` cmdlet. The user can specify the range in which formatting should be applied. The primary usage for this parameter is to be used with editors that request selection formatting.
- (#782, #788) Allman style, Stroustrup style and one true brace style (OTBS) code formatting presets.
- (#790) `Kind` switch to `PSUseConsistentIndentation` rule to provide tabbed indentation.

### Fixed
- (#781, #784) `NewLineAfer` switch behavior in `PSPlaceCloseBrace` rule. When the switch is set to `$false`, the emitted suggested corrections enforce branching control statements to be on the same line as their preceding closing braces. On the other hand when the switch is set to `$true`, the emitted suggested corrections enforce branching controls statements to be on the next line.

Many thanks to @rkeithhill for contributing the _Stroustrup_ style code formatting preset.

## [1.14.1](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.14.1) - 2017-06-12

### Fixed
- (#777) `Invoke-Formatter`
    - Make the cmdlet parameters positional.
    - Updated documentation so that `Get-Help Invoke-Formatter -Syntax` returns a valid syntax.

## [1.14.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.14.0) - 2017-06-09

### Added
- (#772) [`Invoke-Formatter`](https://github.com/PowerShell/PSScriptAnalyzer/blob/4f47ef95edd045029628ba9a4079b1dda19f080f/docs/markdown/Invoke-Formatter.md) cmdlet to format PowerShell scripts. The cmdlet takes a script string and a settings file and outputs formatted script string based on the provided settings. If no settings are provided, the formatter uses the default `CodeFormatting` settings, which can be found at `Settings/CodeFormatting.psd1`.

### Fixed
- (#770) `PSUseIdenticalMandatoryParametersForDSC` rule violation extent. The violation extent covers only the relevant function name, which prior the fix would mark the entire script. This prevented rule suppression from working when the suppression is declared inside `Get/Set/Test` functions.
- (#770) `PSUseIdenticalMandatoryParametersForDSC` behavior to look for mandatory parameters in `Get/Set/Test` functions in a script based resource only if they are declared with attributes, `Key` or `Required`, in the corresponding `mof` file.

## [1.13.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.13.0) - 2017-05-18

### Added
- [`PSUseSupportsShouldProcess`](https://github.com/PowerShell/PSScriptAnalyzer/blob/f92dabdef61b87d5f9f9f2140739c9f3f210b2d8/RuleDocumentation/UseSupportsShouldProcess.md) rule to discourage manual `whatif` and `confirm` parameter declarations.
- Suggested corrections to [`PSProvideCommentHelp`](https://github.com/PowerShell/PSScriptAnalyzer/blob/f92dabdef61b87d5f9f9f2140739c9f3f210b2d8/RuleDocumentation/ProvideCommentHelp.md) rule. The rule can now be configured to:
    - trigger on non-exported functions. But by default, the rule triggers only on exported functions that do have comment help.
    - place the suggested corrections either before a function definition, or at the beginning or end of a function's body.
    - choose between block comment or line comment style of suggested comment help correction.

### Fixed
- `PSAlignAssignmentStatement` to align assignment statements in DSC configurations that have *Undefined DSC Resource* parse errors.

## [1.12.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.12.0) - 2017-05-09

### Added
- [PSAlignAssignmentRuleStatement](https://github.com/PowerShell/PSScriptAnalyzer/blob/cca9a2d7ee35be7322f8c5a09b6c500a0a8bd101/RuleDocumentation/AlignAssignmentStatement.md) rule to align assignment statements in property value pairs (#753).

### Fixed
- `PSAvoidGlobalVars` rule to ignore `$global:lastexitcode` (#752).
- `PSUseConsistentIndentation` to account for backtick on preceding line (#749).
- `PSPlaceCloseBrace` to ignore one-line blocks when `NewLineAfter` switch is on (#748).

## [1.11.1](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.11.1) - 2017-04-04
### Fixed
- CodeFormatting settings file (#727, #728).
- Whitelisted aliases comparison in AvoidUsingCmdletAliases rule (#739).
- PlaceCloseBrace rule behavior for NewLineAfter option (#741).
- UseConsistentIndentation rule to ignore open brace in magic methods (#744).

## [1.11.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.11.0) - 2017-03-01
### Added
- Built-in settings presets to specify settings from command line (#717). Currently, PSSA ships with `PSGallery`, `CodeFormatting`, `DSC`, and other settings presets. All of them can be found in the `Settings/` directory in the module. To use them just pass them as an argument to the `Settings` parameter. For example, if you want to run rules that *powershellgallery* runs, then use the following command.
```powershell
PS> Invoke-ScriptAnalyzer -Path /path/to/your/module -Settings PSGallery
```
- Argument completion for built-in settings presets (#717).
- Argument completion for `IncludeRule` and `ExcludeRule` parameters (#717).
- Option to `PSCloseBrace` rule to add new line after the brace (#713).
- Option to `PSCloseBrace` rule to ignore expressions that have open and close braces on the same line (#706).
- New rule, [PSUseConsistentWhitespace](https://github.com/PowerShell/PSScriptAnalyzer/blob/9a6fe537c11c9c8d7ed872a7464eb192cab00883/RuleDocumentation/UseConsistentWhitespace.md), to check for whitespace style around operators and separators (#702).

### Fixed
- Indentation when pipes precede new lines in a multi-line command expression in `PSUseConsistentIdentation` rule (#705).
- Handling of SubExpressionAsts (`$(...)`) in `PSUseConsistentIdentation` rule (#700).
- Performance issues caused by `get-command` cmdlet (#695).

### Changed
- Settings implementation to decouple it from engine (#717).

## [1.10.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.10.0) - 2017-01-19
### Added
- Three rules to enable code formatting feature in vscode (#690)
    - [PSPlaceOpenBrace](https://github.com/PowerShell/PSScriptAnalyzer/blob/03a6e2b4ee24894bf574a8a8ce911d03680da607/RuleDocumentation/PlaceOpenBrace.md)
    - [PSPlaceCloseBrace](https://github.com/PowerShell/PSScriptAnalyzer/blob/03a6e2b4ee24894bf574a8a8ce911d03680da607/RuleDocumentation/PlaceCloseBrace.md)
    - [PSUseConsistentIdentation](https://github.com/PowerShell/PSScriptAnalyzer/blob/03a6e2b4ee24894bf574a8a8ce911d03680da607/RuleDocumentation/UseConsistentIndentation.md)

### Fixed
- `PSProvideCommentHelp` violation extent (#679)
- `PSAvoidUsingCmdletAliases` rule
    + false positives in DSC configurations (#678)
    + violation extent (#685)
- `PSDSCDSCTestsPresent` rule to check for tests in subdirectories
- `PSUsePSCredentialType` rule (#683)
    + trigger only if invoked from PS version 4 and below
    + violation extent
- `PSAvoidUsingComputerNameHardcoded` rule to ignore `localhost` (#687)
- Performance issues caused by invoking `get-command` method (#692)

## [1.9.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.9.0) - 2016-12-06
### Added
- SuggestedCorrections for ScriptDefinition input (#665)
- [PSAvoidGlobalAliases](https://github.com/PowerShell/PSScriptAnalyzer/blob/278d4fdc56f2c630661b39d242d958f204b69a06/RuleDocumentation/AvoidGlobalAliases.md) and [PSAvoidGlobalFunction](https://github.com/PowerShell/PSScriptAnalyzer/blob/278d4fdc56f2c630661b39d242d958f204b69a06/RuleDocumentation/AvoidGlobalFunctions.md) rules (#658)

### Fixed
- Regular expression in `target` parameter in SuppressMessageAttribute (#638)
- Filtering on severity level for DSC rules (#642)
- PSUseCompatibleCmdlets rule
    + to check for commands in Microsoft.PowerShell.Core snapin
    + to ignore aliases
    + to ignore custom defind commands
- PSUseDeclaredVarsMoreThanAssignments rule to ignore the following special variables (#653)
    + `$PSModuleAutoLoadingPreference`
    + `$InformationPreference`
- PSShouldProcess rule to not enforce `SupportsShouldProcess` if a function calls built-in cmdlet or function that calls `ShouldProcess` (#652).
- PSShouldProcess rule violation extent (#668)
- PSAvoidUsingCmdletAliases rule violation extent (667)
- Column header of `Invoke-ScriptAnalyzer` to be consistent with corresponding property names (#664)
- `ScriptPath` property of external rule violation (#649)

## [1.8.1](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.8.1) - 2016-10-13
### Added
- Catalog file to play nicely with PowerShellGet, version `1.1.0.0`

### Fixed
- [PSUsePSCredentialType](RuleDocumentation/UsePSCredentialType.md) rule to check for attributes on same line without an whitespace between them.
- [PSUseShouldProcessForStateChangingFunctions](RuleDocumentation/UseShouldProcessForStateChangingFunctions.md) rule to check for `start` verb (#634)

## [1.8.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.8.0) - 2016-10-06
### Added
- New [rule](RuleDocumentation/UseCompatibleCmdlets.md) to check cmdlet compatibility between different PowerShell flavors
- New [rule](RuleDocumentation/UseLiteralInitializerForHashtable.md) to warn when using Hashtable constructor
- Feature to [pass parameters to rules from settings file](RuleDocumentation/AvoidAlias.md)
- Feature to [discover settings file](README.md#settings-support-in-scriptanalyzer)
- Enhancement to [PSShouldProcess](RuleDocumentation/UseShouldProcessCorrectly.md) rule to check for ShouldProcess implementation in downstream functions
- A helper [module](Utils/RuleMaker.psm1) to create `C#` based builtin rules

### Fixed
- False negatives for identically named variables (#552)
- Passing `*Ast` arguments to external rules (#614)

### Changed
- PSShouldProcess rule to not check for presence of `ShouldContinue` when `SupportsShouldProcess` is declared

## [1.7.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.7.0) - 2016-08-16

Here are some improvements since the last release.

- Add support for PowerShell Core
    - Tested on PowerShell Core on Windows and Ubuntu 14.04
- Fix `SaveDscDependency` switch implementation, which use fail if more than one parameter is given to `Import-DSCResource` dynamic keyword.
- Add support for external AST based rule suppression
- Fix rule suppression caused by inavlid offsets
- Whitelist `Data` in `PSUseSingularNoun` rule
- Fix rule documentation of `PSDSCExamplesPresent`
- Fix false positives caused by PSD1 files which are not module manifests
    - affects `PSUseToExportFieldsInManifest`, `PSMissingModuleManifestField` and `PSAvoidUsingDeprecatedManifestFields` rules
- Use PlatyPS for generating MAML based documentation
- Add build script to automate building and testing the solution

A big **Thank You!** to the following folks for making PSScriptAnalyzer even better:
- [Kieran Jacobsen (@kjacobsen)](https://github.com/PowerShell/PSScriptAnalyzer/commits/development?author=kjacobsen): Fix rule documentation of `PSDSCExamplesPresent` [PR #591](https://github.com/PowerShell/PSScriptAnalyzer/pull/591)
- [Charlie Schmidt (@charlieschmidt)](https://github.com/PowerShell/PSScriptAnalyzer/commits/development?author=charlieschmidt): Suppress External Rules [PR #585](https://github.com/PowerShell/PSScriptAnalyzer/pull/585)
- [June Blender (@juneb)](https://github.com/PowerShell/PSScriptAnalyzer/commits/development?author=juneb): Add tests for module help [058f65e1](https://github.com/PowerShell/PSScriptAnalyzer/commit/058f65e1f6278222378fedf444eecb2e32865b1e)
- [Shayde Nofziger (@Blackbaud-ShaydeNofziger)](https://github.com/PowerShell/PSScriptAnalyzer/commits/development?author=Blackbaud-ShaydeNofziger): Fix rule name typo and comment [PR #560](https://github.com/PowerShell/PSScriptAnalyzer/pull/560)

## [1.6.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.6.0) - 2016-06-07

### Engine
- Add `SuggestedCorrections` property to `DiagnosticRecord`
    - This property emits text that can rectify the violation which triggered the rule.
    - The primary purpose of this feature is to enable quick-fix scenarios in editors (e.g. vscode)
- Add "Name" as a positional parameter in Get-ScriptAnalyzerRule
- Add a `SaveDscDependency` switch to allow `Invoke-ScriptAnalyzer` to download a DSC Resource module from PSGallery in the event of a `ModuleNotFoundDuringParse` parse error. This feature is available only for PSv5 and above.
- Add `remove` verb to `UseShouldProcessForStateChangingFunctions` rule
- Add a `ScriptPath` property to Diagnostic Record that returns the full path of the script
- Add PSv4 (Windows Server 2012 R2) and PSv3 (Windows Server 2012) to build and test matrix on AppVeyor
- Fix the parsing of `Settings` parameter value when a hashtable is given
- Fix writing error record while running rule suppression
- Fix rule suppression in DSC Configuration definitions
- Fix build and tests on PSv4 and v3

### Rules
- Add suggested corrections feature to the following rules.
    - `PSAvoidUsingCmdletAliases`
    - `PSAvoidUsingPlainTextForPassword`
    - `PSMisleadingBacktick`
    - `PSUseToExportFieldsInManifest`
    - `PSMissingModuleManifestField`
- Fix extent of
    - `PSUseSingularNoun`
    - `PSUseApprovedVerb`
    - `PSAvoidUsernameAndPasswordParams`
- Fix a bug in `PSMissingModuleManifestField` rule caused by .psd1 files that do not contain Hashtable
- Fix documentation of `PSAvoidUsingPlainTextForPassword`

## [1.5.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.5.0) - 2016-03-29

### Engine
- Fixed an engine bug that prevented versioned script rule modules from being loaded
- Fixed loading issues with custom rules that do not have comment-based help to describe the rule
- Fixed a hang issue that appeared when using ScriptAnalyzer engine recursively with a large data set
- Housekeeping: Fixed Appveyor config to use VS 2015 and WMF 5.0 RTM image
- Community Fix: Updated the Initialize API to process the `-Settings` (formerly `-Profile`) parameter
- Perf: Improved rule filtering based on severity before invoking the engine to create tasks (threads)

### Rules
- Fixed `UseToExportFieldsInManifest` rule to improve perf and functionality
- Fixed `AvoidNullOrEmptyHelpMessageAttribute` to use parsed values instead of ast extent
- Fixed inconsistencies in severities of rules
- Community Fix: Fixed false positives on `PSUseApprovedVerbs` when scope is declared as a prefix to the Verb-Noun combination
- Updated messages of `AvoidUsernameAndPasswordParams` rule and `UsePSCredentialType` rule

## [1.4.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.4.0) - 2016-02-16
### Features:
- IncludeRule and ExcludeRule now consume RuleInfo objects

### Rules:
- Rule to validate HelpMessage parameter attribute value
- Rule to suggest module manifest *ToExport field values for optimizing module loading

### Fixes:
#### Engine:
- Fixed bug in engine handling of severity for custom rules - this property was being ignored
- Exclude help files from being Ast parsed

#### Rules:
- Emit accurate ast extents for rules - helps VSCode-PowerShell to mark violations better
- Improved heuristics for Singular noun rule - reduce false positives
- Updated credential rules to be less noisy based on community feedback
- Support for [switch] type along with [boolean] for ShouldContinueWithoutForce rule
- Improved handling of deprecated module manifest fields when PSv2.0 is specified in the manifest

## [1.3.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.3.0) - 2016-01-19
### Features:
- Support for running ScriptAnalyzer on PowerShell version v3 or higher! This means PSv5 is no longer the minimum PS version for ScriptAnalyzer

### Rules:
- [From Community] Rule for warning about backticks that look like line-continuations but are not [MisleadingBacktick rule]
- Ability in default ruleset to recognize PowerShell variable scopes - global, local, script, private
- Ability to use functions as a variable and support for special type of functions like prompt

### Fixes:
#### Rules:
- Fix for recognizing PowerShell preference variable ($OFS) in the default ruleset
- Fix for false positive - PSShouldProcess rule requires boolean value
- Fix to account for function scope prefix
- Raise ReservedParam rule only for exported functions as cmdlets

## [1.2.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.2.0) - 2015-12-16
### Features:
- Support for consuming PowerShell content as streams (-ScriptDefinition)
- ScriptAnalyzer accepts configuration (settings) in the form of a hashtable (-Settings), added sample Settings
- Ability to run default ruleset along with custom ones in the same invocation (-IncludeDefaultRules)
- Recurse Custom Rule Paths (-RecurseCustomRulePath)
- Consistent Engine error handling when working with Settings, Default and Custom Rules

### Rules:
- Rule to detect the presence of default value for Mandatory parameters (AvoidDefaultValueForMandatoryParameter)

### Fixes:
#### Engine:
- Engine update to prevent script based injection attacks
- CustomizedRulePath is now called CustomRulePath â€“ Fixes to handle folder paths
- Fixes for RecurseCustomRulePath functionality
- Fix to binplace cmdlet help file as part of build process
- ScriptAnalyzer Profile is now called Settings
- Fix to emit filename in the diagnosticrecord when using Script based custom rules
- Fix to prevent Engine from calling Update-Help for script based custom rules
- Added additional pester tests to take care of test holes in Custom Rule feature
- Post-build error handling improvements, fixed typos in the project

#### Rules:
- Fixed bug in Positional parameter rule to trigger only when used with >= 3 positional parameters
- Updated keywords that trigger PSAvoidUsingPlainTextForPassword rule
- Updated ProvideDefaultParameterValue rule to AvoidDefaultValueForMandatoryParameter rule
- Deprecate Internal Url rule based on community feedback, identified additional rules to handle hardcoded paths etc
- Added localhost exceptions for HardCodedComputerName Rule
- Update to Credential based rules to validate the presence of CredentialAttribute and PSCredential type

### Documentation:
- Rule & Cmdlet documentation updates â€“ Cmdlet help file addition

## [1.1.1](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.1.1) - 2015-11-03
### Features:
- Support for PSDrives when using Invoke-ScriptAnalyzer
- More robust Profiles feature - better defaulting when supplied with invalid profile - actionable Warnings
- Validated integration with ISESteroids, ISEScriptAnalyzerAddon

### Rules:
- New rule to ensure that a file with missing BOM is encoded in ASCII

### Fixes:
- Doc Updates, Cleaned up source files to reflect accurate comments
- Fix SuppressedOnly switch functionality
- Updated Positional parameter rule to trigger only when used with > 3 positional parameters

## [1.1.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.1.0) - 2015-09-01
### Features:
- Support for using ScriptAnalyzer as a .net library - ScriptAnalyzer APIs
- Support for ScriptAnalyzer Profiles
- Documentation for using Inline Rule Suppression
- Added about help topic file as part of the module

### Rules:
- Rule to checks for UTF8 encoding in help file
- Deprecate Uninitialized Variable rule as per community feedback

### Fixes:
- Fix false positive for UsingInternalURL
- WriteVerbose only when analyzing valid powershell files
- DSCClass rules not being applied when exclude rule is used
- Add host to list of initialized variable
- Exclude external non-powershell applications (Console/GUI) from Positional Parameter rule application
- Additional heuristics for detecting psavoidusingplaintextforpassword rule violation

## [1.0.2](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.0.2) - 2015-06-24
### Features:
- Perf improvements in the Engine to execute rules concurrently.

### Rules:
- New rule to validate the presence of deprecated module manifest fields.
- Removed PSAvoidTrapStatement rule from the builtin set â€“ since this is not deprecated and using trap is a better choice in certain scenarios.

### Fixes:
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

## [1.0.1](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.0.1) - 2015-05-12
### Features:
- Integrated with waffle.io for Project Management.
- Added documentation for writing script rules.

### Rules:
- AvoidUsingWMICmdlet rule: For PowerShell 3.0 and above, usage of WMI cmdlets is not recommended. This rule is to detect WMI cmdlet usage in scripts that are written for PS 3.0 and above.
- DSCTestsPresent rule: Resource module contains Tests folder with tests for given resource.
- UseOutputTypeCorrectly rule: If we can identify the type of an object that is outputted to the pipeline by a cmdlet, then that type must be listed in the OutputType attribute.

### Fixes:
- PSProvideVerboseMessage only throws warnings in non-advanced functions.
- Fix the issue in importing customized rule
- Fix Function Member Ast cast error

## [1.0.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.0.0) - (2015/04/24)
### Features:
- Finalized three levels of Severity - Error/Warning/Information.
- Improved PSScriptAnalyzer engine behavior: emits non-terminating errors (Ex: for failed ast parse) and continues rule application when running on multiple scripts.
- Added wild card supports for rules in Invoke-ScriptAnalyzer and Get-ScriptAnalyzer. Eg. Invoke-ScriptAnalyzer -IncludeRule PSAvoid* will apply all rules starting with PSAvoid* in built in rule assemblies.
- Added -Severity to Get-ScriptAnalyzerRules. Get-ScriptAnalyzer -Severity will filter rules based on the severity given.
- Added Suppression functionality. Users are now able to specify suppression on certain parts of the scripts by specifying "SupressMessageAttribute" in the scripts. More details and documentations will be coming soon in blog posts. Also comes with this feature is the ability for users to display a list of suppressed messages.

### Rules:
- Added DSC Rules for resources including Parameter validation, Usage of standard DSC functions and return type validation. Rule checkings also support for DSC classes. Built-in DSC rules include:
    + UseStandardDSCFunctionsInResource
    + UseIdenticalParametersDSC
    + UseIdenticalMandatoryParametersDSC
    + ReturnCorrectTypesForDSCFunctions
- Added support in the engine to detect DSC configuration/resource files and disable default rule checkings on DSC configuration and resource files.
- UseShouldProcessForStateChangingFunctions - If an advanced function has Verbs like New/Start/Stop/Restart/Reset/Set- that will change system state, it should support ShouldProcess attribute.

### Fixes:
- Improved heuristics to detect usage of Username and Password instead of PSCredential type.
- Improved accuracy in the detection of uninitialized variables.
- Improved error messages to include error line numbers and file names.
- Identified usage of PSBound parameters and PowerShell supplied variables such as $MyInvocation to avoid unnecessary noise in the results returned by some of the built-in rules.
- Fixed terminating errors including "Illegal characters in Path".
