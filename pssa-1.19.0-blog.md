# PSScriptAnalyzer (PSSA) 1.19.0 has been released

## TL;DR; (Too Long; Didn’t Read)

This new minor version brings 5 new rules, the formatter is much faster and other enhancements and fixes. You can get it from the PSGallery [here](https://www.powershellgallery.com/packages/PSScriptAnalyzer/1.19.0). Pending no blocking feedback, the PowerShell VS-Code extension will ship with this version soon as well. For more details, read below or see the full changelog [here](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/CHANGELOG.MD).

## New Script Analysis Rules

There are a total of five new analyzer rules and they were all added by the community!

- [AvoidLongLines](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidLongLines.md) (thanks [Thomas Rayner](https://twitter.com/MrThomasRayner)!): Warns when a line is too long (default is 120 characters) but is not enabled by default.
- [AvoidOverwritingBuiltInCmdlets](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidOverwritingBuiltInCmdlets.md) (thanks [Thomas Rayner](https://twitter.com/MrThomasRayner)!): Warns you if you accidentally try to re-define a built-in cmdlet such as e.g. `Get-Item` by writing code like e.g. `function Get-Item { }`.
- [UseUsingScopeModifierInNewRunspaces](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseUsingScopeModifierInNewRunspaces.md) (thanks [Jos Koelewijn](https://twitter.com/Jawz_84)!): Warns you when trying to incorrectly reference a variable that is not defined directly in the scope of the current scriptblock. This applies e.g. to the `-Command` parameter of `Invoke-Command` or `-Parallel` of `ForEach-Object` (added in PowerShell 7, see blog [here](https://devblogs.microsoft.com/powershell/powershell-foreach-object-parallel-feature/)). It recommends to use the `$using` scope modifier to reference the variable correcty.
- [UseProcessBlockForPipelineCommand](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseProcessBlockForPipelineCommand.md) (thanks [Matt McNabb](https://twitter.com/mcnabbmh)!): Warns when a function declares support for pipeline input (via the `[Parameter(ValueFromPipeline)]` attribute) that the function needs to declare a `process { }` block.
- [ReviewUnusedParameter](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/ReviewUnusedParameter.md) (thanks [Matt McNabb](https://twitter.com/mcnabbmh)!): Warns on parameters declared in a script, scriptblock, or function scope that have not been used in that scope. You can think of it as the equivalent of the `UseDeclaredVarsMoreThanAssignments` rule but for parameters instead of variables.

The added computational work for those rules are being compensated for by a performance improvement in the `AvoidTrailingWhitespace` rule, therefore the speed of running `Invoke-ScriptAnalyzer` for all rules is roughly the same compared to the previous version of PSSScriptAnalyzer.

## Formatter

### Performance

Several improvements have been made to the formatting rules and the engine that should result in the formatter being multiple times faster, especially for large scripts. The improvements were:

- Reduced initialisation overhead.
- Improved efficiency of formatting rules, which addresses the scaling problems that were seen for large scripts.
- Reduce the number of times the script text has to be re-parsed. In its current architecture, the formatter has to re-parse the script after every rule run and every applied correction. When a rule has no violations then the parsed AST and tokens are being recycled. This leads to faster formatting when no or only some correction have to be made.

To give a specific example, running the formatter on [PowerShell](https://github.com/PowerShell/PowerShell)'s [build.psm1](https://github.com/PowerShell/PowerShell/blob/master/build.psm1) module, which has over 3000 lines, we've measure the following times for a 'warm' run:

- Version 1.18.3 takes around 1,250 ms.
- Version 1.19.0 takes around 550 ms.
- Version 1.19.0 takes around 170 ms on the pre-formatted file.

The reason why we measure a 'warm' run is because in the editor scenario, the PowerShell extension will have likely already finished a run `Invoke-ScriptAnalyzer` in the background on the script, which means the [CommandInfo](https://docs.microsoft.com/dotnet/api/system.management.automation.commandinfo) cache has already been populated. Although the default set of formatting rules in PowerShell's VS-Code extension doesn't need the CommandInfo cache and therefore there wouldn't be much of a difference between cold and warm for most users, the following 2 optional rules do use the CommandInfo:

- [UseCorrectCasing](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseCorrectCasing.md): Controlled via the `powershell.codeFormatting.useCorrectCasing` PowerShell VS-Code extension setting.
- [AvoidUsingCmdletAliases](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingCmdletAliases.md): Controlled via the `powershell.codeFormatting.autoCorrectAliases` PowerShell VS-Code extension setting.

### New Formatter features

- Parameter Casing: The [UseCorrectCasing](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseCorrectCasing.md) rule now correct also the casing of parameters and not just the cmdlet names.
- The `PipelineIndentationStyle` setting of `UseConsistentIndentation` has a new option now and will also become the default in PowerShell's VS-Code extension. This new option is named `None` and means that it does not change the user's pipeline indentation at all and therefore change this feature to be an opt-in scenario. For the previous non-default settings `IncreaseIndentationForFirstPipeline` and `IncreaseIndentationAfterEveryPipeline` all new user reported bugs are fixed now and therefore we encourage you to please try it out again.
- The [UseConsistentWhitespace](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseConsistentWhitespace.md) option `CheckPipe` has not been changed to only ADD whitespace around the pipe if it is missing but not remove extranous whitespace. This is because some people prefer to line up their pipelines in Pester tests. For anyone who still wants to trim redundant whitespace around pipes, the new `CheckPipeForRedundantWhitespace` option has been provided. For VS-Code users this means that we plan to split the existing `powershell.codeFormatting.whitespaceAroundPipe` (enabled by default) setting into 2 new ones: `powershell.codeFormatting.addWhitespaceAroundPipe` (enabled by default) and `powershell.codeFormatting.trimWhitespaceAroundPipe` (disabled by default).
- The [UseConsistentWhitespace](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseConsistentWhitespace.md) option `CheckParameter` has been added and can be controlled via the VS-Code setting `powershell.codeFormatting.whitespaceBetweenParameters`.

## Other improvements

- The compatibility rules were updated to includes profiles for PowerShell 7, support single number version strings now as well and also have added analysis for PowerShell 7 syntax (null-conditional method invocation, null-coalescing operator, ternary expression and pipeline chain operator).
- [AvoidAssignmentToAutomaticVariable](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidAssignmentToAutomaticVariable.md) rule was enanced to include now not only warn on read-only [automatic variables](https://docs.microsoft.com/powershell/module/microsoft.powershell.core/about/about_automatic_variables), but also  other automatic variables that can but should not be assigned to, which also includes the commonly misused `$input` variable.
- The minimum supported version of PowerShell Core is `6.2.4` now but please bear in mind that support for Powershell `6.2` itself ends on September 04 2020, see support policy [here](https://docs.microsoft.com/en-us/powershell/scripting/powershell-support-lifecycle?view=powershell-7#powershell-releases-end-of-life).
- Our CI system has been migrated to use multi-stage Azure Pipelines, meaning that every commit gets tested against `Windows` (Server 2016 and 2019), `Ubuntu` (`16.04` and `18.04`) and `macOS` (`10.14` and `10.15`) for PowerShell 7 and also Windows PowerShell 5.1. The previous AppVeyor build still provides coverage for PowerShell 4 and before the release a manual test was executed to guarantee functionality for PowerShell 3.

## Outlook

As mentioned in multiple, previous posts, the PowerShell teams is looking at a partial re-write of PSScriptAnalyzer, which will likely require a bump of the major version. Therefore `1.19.0` might be the last version of `1.x`, maybe with the exception of a patch. In version `2.x`, support for PowerShell 3 and 4 is likely going to drop.

On behalf of the Script Analyzer team,

[Christoph Bergmeister](https://twitter.com/CBergmeister), Project Maintainer from the community, [BJSS](https://www.bjss.com/)

[Jim Truher](https://twitter.com/jwtruher), Senior Software Engineer, Microsoft

[Rob Holt](https://twitter.com/rjmholt), Software Engineer on the PowerShell team, Microsoft
