# CHANGELOG

## [1.24.0](https://github.com/PowerShell/PSScriptAnalyzer/releases/tag/1.24.0)

### What's Changed
#### Breaking Changes

Minimum required PowerShell version raised from 3 to 5.1
* Drop v3 and v4 support from build by @andyleejordan in https://github.com/PowerShell/PSScriptAnalyzer/pull/2081

#### New Features

* Add new options (enabled by default) to formatting rule `UseCorrectCasing` to also correct operators, keywords and commands - Add UseConsistentCasing by @Jaykul in https://github.com/PowerShell/PSScriptAnalyzer/pull/1704

#### Enhancements

* PSAlignAssignmentStatement: Ignore hashtables with a single key-value pair by @liamjpeters in https://github.com/PowerShell/PSScriptAnalyzer/pull/1986
* Use `RequiredResource` hashtable to specify PowerShell module versions by @andyleejordan in https://github.com/PowerShell/PSScriptAnalyzer/pull/2053
* Set exit code of `Invoke-ScriptAnalyzer -EnableExit` to total number of diagnostics (#2054) by @MatejKafka in https://github.com/PowerShell/PSScriptAnalyzer/pull/2055
* PSAvoidAssignmentToAutomaticVariable: Ignore when a Parameter has an Attribute that contains a Variable expression by @liamjpeters in https://github.com/PowerShell/PSScriptAnalyzer/pull/1988
* Trim unnecessary trailing spaces from string resources in Strings.resx by @XPlantefeve in https://github.com/PowerShell/PSScriptAnalyzer/pull/1972
* Do not print summary repeatedly for each logger by @MatejKafka in https://github.com/PowerShell/PSScriptAnalyzer/pull/2058
* Make Settings type detection more robust by @Tadas in https://github.com/PowerShell/PSScriptAnalyzer/pull/1967
* Add foreach Assignment to AvoidAssignmentToAutomaticVariable by @PoshAJ in https://github.com/PowerShell/PSScriptAnalyzer/pull/2021
* Invoke-ScriptAnalyzer: Stream diagnostics instead of batching by @MatejKafka in https://github.com/PowerShell/PSScriptAnalyzer/pull/2062
* Invoke-ScriptAnalyzer: Print summary only once per invocation by @MatejKafka in https://github.com/PowerShell/PSScriptAnalyzer/pull/2063
* Invoke-ScriptAnalyzer: Include parse errors in reported error count by @MatejKafka in https://github.com/PowerShell/PSScriptAnalyzer/pull/2069
* Add exception message for missing rules by @Tadas in https://github.com/PowerShell/PSScriptAnalyzer/pull/1968

#### Bug Fixes

* Update links in module manifest by @martincostello in https://github.com/PowerShell/PSScriptAnalyzer/pull/2034
* Fix incorrect `-ReportSummary` Pester test grouping by @MatejKafka in https://github.com/PowerShell/PSScriptAnalyzer/pull/2057
* Fixed erroneous PSUseDeclaredVarsMoreThanAssignments for some globals variables by @John-Leitch in https://github.com/PowerShell/PSScriptAnalyzer/pull/2013
* PSReservedParams: Make severity Error instead of Warning by @liamjpeters in https://github.com/PowerShell/PSScriptAnalyzer/pull/1989
* PSUseConsistentIndentation: Check indentation of lines where first token is a LParen not followed by comment or new line by @liamjpeters in https://github.com/PowerShell/PSScriptAnalyzer/pull/1995
* PSUseConsistentWhitespace: Correctly fix whitespace between command parameters when parameter value spans multiple lines  by @liamjpeters in https://github.com/PowerShell/PSScriptAnalyzer/pull/2064
* PSAvoidTrailingWhitespace: Rule not applied when using formatter + single character lines with trailing whitespace are truncated by @liamjpeters in https://github.com/PowerShell/PSScriptAnalyzer/pull/1993
* PSUseConsistentWhitespace: Ignore whitespace between separator and comment by @liamjpeters in https://github.com/PowerShell/PSScriptAnalyzer/pull/2065
* PSReviewUnusedParameter false positive for ValueFromPipeline by @liamjpeters in https://github.com/PowerShell/PSScriptAnalyzer/pull/2072
* Change severity of UseCorrectCasing to be Information by @bergmeister in https://github.com/PowerShell/PSScriptAnalyzer/pull/2082

#### Process Changes

* Copy more files to module root by @andyleejordan in https://github.com/PowerShell/PSScriptAnalyzer/pull/2037
* Upgrade to .NET 8 since .NET 6 is past EOL by @andyleejordan in https://github.com/PowerShell/PSScriptAnalyzer/pull/2073
* Use -NoProfile when invoking pwsh in Pester tests by @MatejKafka in https://github.com/PowerShell/PSScriptAnalyzer/pull/2061
* Add GitHub Actions Ubuntu's dotnet path by @andyleejordan in https://github.com/PowerShell/PSScriptAnalyzer/pull/2080
* Update README.md with recent upgrade to .NET 8 by @bergmeister in https://github.com/PowerShell/PSScriptAnalyzer/pull/2076
* Update CHANGELOG.MD with 1.23.0 release notes by @bergmeister in https://github.com/PowerShell/PSScriptAnalyzer/pull/2078
* Bring back Codespaces by @bergmeister in https://github.com/PowerShell/PSScriptAnalyzer/pull/2077
* Update SMA version to 7.4.7 by @bergmeister in https://github.com/PowerShell/PSScriptAnalyzer/pull/2075
* Test PowerShell Preview in CI by @andyleejordan in https://github.com/PowerShell/PSScriptAnalyzer/pull/2070
* Backport MSDocs changes by @bergmeister in https://github.com/PowerShell/PSScriptAnalyzer/pull/2085
* Document new optional parameters added to UseCorrectCasing by @bergmeister in https://github.com/PowerShell/PSScriptAnalyzer/pull/2086

### New Contributors
* @martincostello made their first contribution in https://github.com/PowerShell/PSScriptAnalyzer/pull/2034
* @MatejKafka made their first contribution in https://github.com/PowerShell/PSScriptAnalyzer/pull/2055
* @XPlantefeve made their first contribution in https://github.com/PowerShell/PSScriptAnalyzer/pull/1972
* @John-Leitch made their first contribution in https://github.com/PowerShell/PSScriptAnalyzer/pull/2013
* @Tadas made their first contribution in https://github.com/PowerShell/PSScriptAnalyzer/pull/1967
* @PoshAJ made their first contribution in https://github.com/PowerShell/PSScriptAnalyzer/pull/2021
* @Jaykul made their first contribution in https://github.com/PowerShell/PSScriptAnalyzer/pull/1704

**Full Changelog**: https://github.com/PowerShell/PSScriptAnalyzer/compare/1.23.0...1.24.0

## [1.23.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.23.0) - 2024-10-09

### What's Changed
* Adding OneBranch pipeline YAML config file for OSS_Microsoft_PSSA-Official by @adityapatwardhan in https://github.com/PowerShell/PSScriptAnalyzer/pull/1981
* Update format and grammar of AvoidUsingAllowUnencryptedAuthentication by @sdwheeler in https://github.com/PowerShell/PSScriptAnalyzer/pull/1974
* Move to OneBranch Signing and SBOM generation by @TravisEz13 in https://github.com/PowerShell/PSScriptAnalyzer/pull/1982
* Sync rule docs changes by @sdwheeler in https://github.com/PowerShell/PSScriptAnalyzer/pull/1985
* Sync docs changes from MicrosoftDocs/PowerShell-Docs-Modules#213 by @sdwheeler in https://github.com/PowerShell/PSScriptAnalyzer/pull/1987
* Update CHANGELOG for 1.22.0 release by @sdwheeler in https://github.com/PowerShell/PSScriptAnalyzer/pull/1990
* Update Code of Conduct by @andyleejordan in https://github.com/PowerShell/PSScriptAnalyzer/pull/2002
* Update default type definition of `RuleInfo` by @liamjpeters in https://github.com/PowerShell/PSScriptAnalyzer/pull/2011
* PSUseConsistentWhitespace: Handle redirect operators which are not in stream order by @liamjpeters in https://github.com/PowerShell/PSScriptAnalyzer/pull/2001
* Setup GitHub Actions CI by @andyleejordan in https://github.com/PowerShell/PSScriptAnalyzer/pull/2018
* Setup new OneBranch pipeline by @andyleejordan in https://github.com/PowerShell/PSScriptAnalyzer/pull/2027
* Bump SMA version by @andyleejordan in https://github.com/PowerShell/PSScriptAnalyzer/pull/2028
* Package updates by @andyleejordan in https://github.com/PowerShell/PSScriptAnalyzer/pull/2030
* v1.23.0: Update version for new release by @andyleejordan in https://github.com/PowerShell/PSScriptAnalyzer/pull/2032
* Migrate release pipeline to DeployBox by @andyleejordan in https://github.com/PowerShell/PSScriptAnalyzer/pull/2033

### New Contributors
* @adityapatwardhan made their first contribution in https://github.com/PowerShell/PSScriptAnalyzer/pull/1981

**Full Changelog**: https://github.com/PowerShell/PSScriptAnalyzer/compare/1.22.0...1.23.0

## [1.22.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.22.0) - 2024-03-05

Minimum required version when using PowerShell 7 is now `7.2.11`.

### New Rule

- Add AvoidUsingAllowUnencryptedAuthentication by @MJVL in (#1857)
- Add the AvoidExclaimOperator rule to warn about the use of the ! negation operator. Fixes (#1826) by
  @liamjpeters in (#1922)

### Enhancements

- Enable suppression of PSAvoidAssignmentToAutomaticVariable for specific variable or parameter by
  @fflaten in (#1896)
- Upgrade to use .NET 6 since PowerShell 7.0 is now out out of support by @bergmeister in (#1873)
- Convert UseSingularNouns to configurable rule and add Windows to allowlist by @MJVL in (#1858)
- Add ErrorView to SpecialVars.cs by @ewisniew0 in (#1865)
- Allow suppression of  PSUseSingularNouns for specific function by @fflaten in (#1903)
- Adding ToString() methods to [CorrectionExtent] and [DiagnosticRecord] by @StartAutomating in (#1946)
- Add PSNativeCommandUseErrorActionPreference preference variable by @aelij in (#1954)
- AvoidUsingPositionalParameter: Check if command has parameters to avoid having az in default
  CommandAllowList by @bergmeister in (#1850)
- PSReviewUnusedParameter: Add CommandsToTraverse option by @FriedrichWeinmann in (#1921)

### Fixes

- Prevent NullReferenceException for null analysis type. by @hubuk in (#1949)

### Build & Test, Documentation and Maintenance

- UseApprovedVerbs.md: Backport minor change of PR 104 in PowerShell-Docs-Modules by @bergmeister in
  (#1849)
- Improve Pester bootstrap logic for CI by @bergmeister in (#1853)
- Bump .NET SDK from 3.1.419 to 3.1.424 by @bergmeister in (#1852)
- AvoidLongLines: Make internal function DiagnosticSeverity private by @bergmeister in (#1851)
- SupportsShouldProcess.md: Fix Typo - MicrosoftDocs backport of PR 121 there by @sdwheeler in (#1869)
- Minor test fix for UseCorrectCasing rule by @kilasuit in (#1885)
- Make Invoke-Formatter test case assertion fail in case of incorrect casing by @alexandear in (#1888)
- Fix `AvoidUsingDoubleQuotesForConstantString` information in overview README by @michaeltlombardi
  in (#1883)
- Update dependabot reviewers to remove Rob by @fflaten in (#1897)
- Fix typo in AvoidUsingPlainTextForPassword error message: 'to' being repeated two times by
  @ALiwoto in (#1902)
- CI: Use new Ubuntu 22.04  image and remove deprecated Ubuntu 18.04 by @bergmeister in (#1847)
- Change double quotes to single where possible by @sdwheeler in (#1911)
- Backport MicrosoftDocs PR 143 by @sdwheeler in (#1910)
- Fix typos in rules documentation by @sdwheeler in (#1913)
- add demand for compliance job by @TravisEz13 in (#1920)
- FabricBot: Onboarding to GitOps.ResourceManagement because of FabricBot decommissioning by
  @microsoft-github-policy-service in (#1925)
- Sync changes from Docs repository by @sdwheeler in (#1929)
- Developer documentation fix and message fix of
  PossibleIncorrectUsageOfRedirectionOperatorDescription by @JoelTipke in (#1928)
- Documentation corrections for AvoidUsingPositionalParameters by @ImportTaste in (#1917)
- Update minimum PowerShell Core version to 7.2.11 as 7.0 is now EOL by @bergmeister in (#1872)
- Remove dead code and simplify by @bergmeister in (#1856)
- PSReservedParams - link about_CommonParameters by @petervandivier in (#1908)
- Generate strongly typed resources as part of build by @bergmeister in (#1855)
- Bump Newtonsoft.Json to 13.0.3 by @dependabot in (#1866)
- Use latest .NET 6.0 SDK patch version and update devcontainer to use .NET 6 as well by
  @bergmeister in (#1955)
- Bump Microsoft.Management.Infrastructure from 1.0.0 to 3.0.0 for PowerShell 7 only by @dependabot
  in (#1947)
- Bump version from 1.21.0 to 1.22.0 by @bergmeister in (#1965)
- Remove Appveyor badge from main README by @bergmeister in (#1962)
- Do not hard code common parameters in module help test any more by @bergmeister in (#1963)

### New Contributors

- @fflaten made their first contribution in (#1897)
- @ALiwoto made their first contribution in (#1902)
- @microsoft-github-policy-service made their first contribution in (#1925)
- @JoelTipke made their first contribution in (#1928)
- @ImportTaste made their first contribution in (#1917)
- @liamjpeters made their first contribution in (#1922)
- @petervandivier made their first contribution in (#1908)
- @ewisniew0 made their first contribution in (#1865)
- @StartAutomating made their first contribution in (#1946)
- @aelij made their first contribution in (#1954)
- @FriedrichWeinmann made their first contribution in (#1921)

**Full Changelog**: <https://github.com/PowerShell/PSScriptAnalyzer/compare/1.21.0...1.22.0>

## [1.21.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.21.0) - 2022-09-27

### New Rule

- Add AvoidMultipleTypeAttributes rule (#1705) (thanks @hankyi95)
- Add the AvoidSemicolonsAsLineTerminators rule to warn about lines ending with a semicolon. Fix
  (#824) (#1806) (thanks @tempora-mutantur)
- Add AvoidUsingBrokenHashAlgorithms (#1787) (thanks @MJVL)

### Enhancements

- Also return suggestion to use PSCredential for AvoidUsingPlainTextForPassword rule (#1782) (by @bergmeister)
- Invoke-Formatter: Accept input from pipeline (#1763) (by @bergmeister)
- Make messages of UseCorrectCasing more detailed (#1843)
- Exclude automatic variable FormatEnumerationLimit from analysis by PSAvoidGlobalVars and
  PSUseDeclaredVarsMoreThanAssignments (#1836) (by @bergmeister)
- PSAvoidUsingPositionalParameters: Do not warn on AZ CLI (#1846) (by @bergmeister)

### Fixes

- Fix PSUseConsistentIndentation.PipelineIndentation.None to not remove code when the previous line ended with a pipe (#1746) (by @bergmeister)
- Fix edge case of UseConsistentIndentation where child pipeline is on the same line as hashtable (#1838) (by @bergmeister)
- Skip OpenBrace check when preceded by Dot token (#1750) (by @bergmeister)
- Fix NRE (NullReferenceException) when custom rules omit optional properties in diagnostics (#1715) (by @rjmholt)

### Build, Documenation and Maintenance

- Tweak documentation for AvoidUsingBrokenHashAlgorithms (#1829) (thanks @michaeltlombardi)
- Set MaxDepth in JsonSerializerSettings for more secure handling of exceptional conditions in Newtonsoft.Json (#1824) (by @bergmeister)
- Use latest Newtonsoft.Json version for Windows PowerShell builds and not exclude it any more in Dependabot (#1825) (by @bergmeister)
- Correct pipeline acceptance docs for Invoke-Formatter.md (#1833) (by @bergmeister)
- Sync remaining docs changes from Microsoft docs (#1835) (by @bergmeister)
- (MAINT) Update URLs for site rebrand from docs.microsoft.com to learn.microsoft.com (#1844) (by @sdwheeler)
- Reformat note block (#1837) (by @sdwheeler)
- Update AvoidMultipleTypeAttributes.md with example that has runtime error (#1831) (by @bergmeister)
- Fix minimum PowerShell version to be 7.0.11 instead of 7.1.7 (#1830) (by @bergmeister)
- Add `.github/fabricbot.json` (#1812)
- Sync changes from docs repo (#1814) (by @sdwheeler)
- Fix name of PowerShell 5.1 test stage in CI (#1820) (by @bergmeister)
- CI: Retry test tasks in cas1e of failure to reduce sporadic failures (#1770)
- Upgrade from net452 to net462 for Windows PowerShell (#1789)
- Move issue template config into folder (#1804) (thanks @michaeltlombardi)
- Bump version from 1.20.0 to 1.21.0 (#1796) (by @bergmeister)
- Bump .NET SDK and SMA patch version (#1795) (by @bergmeister)
- Be sure not to clobber existing files when installing dotnet. (#1788) (by @JamesWTruher)
- Add link to issues chooser for reporting docs issues (#1794) (by @sdwheeler)
- Update README and delete docs that were migrated (#1790) (by @sdwheeler)
- Fix errors in ShouldProcess rule document (#1766) (thanks @masaru-iritani)
- Replicate changes from docs repo (#1781) (by @sdwheeler)
- Update Windows CI images as windows-2016 has been deprecated (#1784) (by @bergmeister)
- Use latest mac image rather than a specific version. (#1777) (by @JamesWTruher)
- Add docs migration notice & fix formatting (#1779) (by @sdwheeler)
- use compliant build environment for release build. (#1776) (by @JamesWTruher)
- Enable SBOM creation for script analyzer (#1762) (by @JamesWTruher)
- Fix links to work on GitHub (#1738) (by @andschwa)
- Update AvoidUsingWMICmdlet.md (#1737) (thanks @C0smin)
- Fix broken doc link in README (#1735) (thanks @AndrewRathbun)
- Add metadata to docs (#1731) (by @sdwheeler)
- Remove Ubuntu 16.04 from test matrix (#1733) (by @rjmholt)
- Use PowerShell1ES pool for official build (#1719) (by @JamesWTruher)
- Update cmdlet docs for 1.20.0 (#1726) (by @sdwheeler)
- Fixes (#1720) - move rule docs and update tests (#1724) (by @sdwheeler)
- Update rule docs (#1711)  (by @sdwheeler)

## [1.20.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.20.0) - 2021-08-20

### Fixes

- Replace unhelpful warning around `process` aliasing `Get-Process` with warning about misused syntax (#1638) (by @bergmeister)
- Fix `FunctionInfo` fallback AST attribute analysis for UseShouldProcessCorrectly (#1659) (thanks @hubuk)
- Do not increase indentation after a left parenthesis if the previous token is a newline and the next token is not a newline (#1469) (by @bergmeister)
- UseConsistentWhitespace - CheckOpenBrace setting: Do not warn when being preceded by open parenthesis (#1633) (by @bergmeister)

### New Rule

- Make UseSingularNouns rule work in PowerShell 7 (#1627) (by @bergmeister)
- UseConsistentWhitespace: Create option to ignore assignment operator inside hashtable (#1566) (thanks @daviesj)

### Miscellaneous

- Implement -IncludeSuppressions parameter (#1701) (by @rjmholt)
- Combine multiple suppressions applied to the same diagnostic (#1699) (by @rjmholt)
- Use dependabot to keep dependencies up to date (#1664) (by @rjmholt)
- Add reviewers and explicit dependency exclusions to dependabot (#1676) (by @rjmholt)
- Remove explicit registries in dependabot (#1671) (by @rjmholt)

### Documentation

- Link to PSScriptAnalyzer rule documentation from the README (#1642) (thanks @bbodenmiller)
- Add HelpInfoUri to module manifest (#1651) (by @sdwheeler)
- Add documentation around CustomRulePath in Settings file to README.md (#1636) (thanks @johlju)
- Use allowlist and blocklist terminology, supply alternate configuration key for PSAvoidUsingCmdletAliases (#1604) (by @bergmeister)
- Fix suppression example errors in README (#1593) (by @rjmholt)
- Fix AvoidLongLines documentation example (#1554) (thanks @floh96)
- Remove broken waffle.io links from readme.md (#1558) (thanks @clcaldwell)
- Fix typo in AvoidUsingCmdletAliases documentation (#1590) (thanks @rubengonzalez-dev)

### Testing

- Fix compatibility rule tests (#1697) (by @rjmholt)
- Fix missing space in AvoidPositionalParameter test asset (#1573) (by @jameswtruher)

### Development/Build Improvements

- Fix build module tests (#1654) (thanks @clcaldwell)
- Move to new signing process for release build and prep for 1.20.0 (#1625) (by @jameswtruher)
- Handle cases where the signature generator fails (#1661) (by @jameswtruher)
- Update .NET SDK to 3.1.408 (#1630) (by @bergmeister)
- Simplify devcontainer setup and resolve git line ending conflicts (#1637) (by @bergmeister)
- Speedup CI by bootstrapping PowerShell module installations in background and in parallel, whilst the .NET SDK is being installed (#1634) (by @bergmeister)
- Upgrade release Dockerfile from Ubuntu 16.04 to 20.04 (#1631) (by @bergmeister)
- Remove old powershell-core NuGet feed, which sometimes causes build failures (#1632) (by @bergmeister)
- Remove reference to myget.org as a package resource (#1640) (by @jameswtruher)
- Add Ubuntu 20.04 to build matrix (#1628) (by @bergmeister)
- Remove conditional compilation for PSv6/netstandard2.0 for Engine and Rules projects (#1600) (by @bergmeister)
- Upgrade Microsoft.CSharp, Microsoft.Win32.Registry and System.Reflection.TypeExtensions from 4.5.0 to 4.7.0 (#1599) (by @bergmeister)
- Use PowerShell preview extension for Github Codespaces (#1596) (by @bergmeister)
- Update SMA reference for netcoreapp3.1 to from 7.0.0 to 7.0.3 (#1597) (by @bergmeister)
- 🧹 Make usage of Microsoft.Management.Infrastructure consistent and reference it only once in CrossCompatibility project (#1601) (by @bergmeister)
- Update launch.json (#1603) (by @bergmeister)
- Make CI fail if tests fail and fix failing tests by making them Pester v5 compatible (#1553) (coauthored by @bergmeister)

## [1.19.1](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.19.1) - 2020-07-28

### Fixes

- `UseCorrectCasing`: Do not use CommandInfoCache when CommandInfoParameters property throws due to runspace affinity problem of PowerShell engine (#1523) (by @bergmeister)
- `ReviewUnusedParameter`: Do not trigger when MyInvocation.BoundParameters or PSCmdlet.MyInvocation.BoundParameters is used (#1520) (Thanks @jegannathanmaniganadan!)
- `PipelineIndentationStyle.None`: Fix bug that caused incorrect formatting in hashtables (#1497) (by @bergmeister)
- `UseUsingScopeModifierInNewRunspaces`: Fix ArgumentException when the same variable name is used in 2 different sessions. (#1493) (by @bergmeister)
- `UseConsistentWhitespace`
  - Check previous token only if it starts on the same line (#1491) (by @bergmeister)
  - Fix CheckParameter bug when using interpolated string (#1498) (by @bergmeister)

### New Rule

- New rule (disabled by default): [AvoidUsingDoubleQuotesForConstantString](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingDoubleQuotesForConstantString.md) (#1470) (by @bergmeister)

### Miscellaneous

- Set Assembly version attribute (#1549) (by @JamesWTruher)

### Documentation

- Remove references to Windows in Introduction of Readme.md (#1509) (Thanks @hjorslev!)

### Testing

- Pester v5 (#1444) (by @bergmeister)

### Development improvements

- Include PDB files in Debug build (#1535) (by @bergmeister)
- Add more useful launch configurations (#1499) (by @bergmeister)
- Make .Net Core version in Docker image of .devcontainer match the one in global.json (#1494) (by @bergmeister)

## [1.19.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.19.0)

### New Rules

- UseUsingScopeModifierInNewRunspaces (#1419) (thanks @Jawz84!)
- ReviewUnusedParameter (#1382) (thanks @mattmcnabb!)
- AvoidOverwritingBuiltInCmdlets (#1348) (thanks @thomasrayner!)
- UseProcessBlockForPipelineCommand (#1373) (thanks @mattmcnabb!)
- AvoidLongLines (#1329) (thanks @thomasrayner!)

### Rule Enhancements

- Add more automatic variables to PSAvoidAssignmentToAutomaticVariables that are not read-only but should still not be assigned to in most cases (#1394) (by @bergmeister)

### Compatibility Rules

- Make CompatibilityCollector able to parse a single version String (#1446) (by @bergmeister)
- Update compatibility profiles for PowerShell 7 (#1429) (by @rjmholt)
- Ps7 syntax (#1426) (by @rjmholt)
- Fix ps3 syntax check (#1395) (by @rjmholt)
- Add PS 7 to UseCompatibleSyntax (#1331) (by @rjmholt)
- Fix UseCompatibleSyntax configuration doc ([e37daeb](https://github.com/PowerShell/PSScriptAnalyzer/commit/e37daeb62d7ae6267b1a2c78a50404c8ab271786)) (by @rjmholt)
- Update UseCompatibleTypes docs with more info ([42f5e58](https://github.com/PowerShell/PSScriptAnalyzer/commit/42f5e58ad689fd3871bf4d9c5977b9352d1d4a1f)) (by @rjmholt)
- UseCompatibleCommands doc improvements ([7874b55](https://github.com/PowerShell/PSScriptAnalyzer/commit/7874b555643315dc3d8c00394fac81e0684504ff)) (by @rjmholt)
- Update AzF profile to be useful (#1341) (by @rjmholt)

### Formatter

- UseCorrectCasing
  - Correct casing of parameters as well in PSUseCorrectCasing formatter rule (#1391) (by @bergmeister)
  - Fix temporarily introduced bug (PR #1391) for UseCorrectCasing (#1408) (by @bergmeister)
- UseConsistentIndentation
  - When non-default options are used, cater for the case of a comment between pipe and newline (#1463)
  - Add PipelineIndentationStyle.None option for scenarios where indentation is custom, inconsistent or the user does not like any of the 3 pipeline indentation styles (#1399) (by @bergmeister)
  - Fix complex case when PipelineIndentation is not set to default (NoIndentation) (#1359) (by @bergmeister)
- UseConsistentWhitespace
  - Split CheckPipe feature of trimming redundant whitespace out into option CheckPipeForRedundantWhiteSpace (#1413) (by @bergmeister)
  - Set default of CheckParameter configuration setting to false to ensure that setting won't be enabled by default if an old version of the VS-Code extension uses a new version of - PSScriptAnalyzer (1.19) (#1411) (by @bergmeister)
  - Remove redundant whitespace between parameters with new option (disabled by default) in UseConsistentWhitespace (#1392) (by @bergmeister)
  - Ignore empty hashtable for CheckInnerBrace configuration (#1349) (by @bergmeister)

### Performance

The Formatter can now be multiple times faster depending on the use case and especially scaling problems were addressed that showed when analyzing large scripts with thousands of lines (a 3000 line test script was used in the below performance figures). Optimisations were to

- Eliminate initialization overhead, which can lead to a reduced time of up to 50%
- Optimize operations in rules to make them scale better, another improvement of around 50%
- The formatter used to re-parse the script in between every rule run but this is skipped now if the previous rule did not emit a `DiagnosticRecord` as it means the script text hasn't changed and doesn't need updating. Therefore formatting will be faster on scripts that need no to little changes.

A small improvement was made to a script analysis rule as well but the gain is offset by the addition of the new rules in this release.

The PRs for those improvements are:

- Eliminate Regex overhead in AvoidTrailingWhitespace -> Speedup of 5% (PowerShell 5.1) or 2.5 % (PowerShell 7.1-preview.2) (#1465) (by @bergmeister)
- Formatter: Recycle parsed AST and tokens in between rule invocations when no correction was applied to improve performance (#1462) (by @bergmeister)
- Improve performance of UseConsistentIndentation even more > another 10% speedup for formatter (#1461) (by @bergmeister)
- Improve performance of UseConsistentIndentation -> 35% speedup for formatter (#1458) (by @bergmeister)
- Lazy initialisation of LinkedList in TokenOperations constructor -> 7% performance gain for formatter (#1453) (by @bergmeister)
- Invoke-Formatter: Skip VariableAnalysis, which is not needed to yield a 50% performance improvement (#1451) (by @bergmeister)

### Fixes

- Fix edge case of PSUseConsistentIndentation for non-default value (IncreaseIndentationForFirstPipeline/IncreaseIndentationAfterEveryPipeline) (#1423) (by @bergmeister)
- Fix rule suppression ID in compat rules (#1432) (by @rjmholt)
- Changes to build analyzer for PS7 and PS6 and ship in separate directories and bump version to 1.19.0 (#1425) (by @JamesWTruher)
- Use AST to determine SupportsShouldProcess when an error is thrown (#1397) (by @rjmholt)
- Fix false positive of AvoidAlias rule for implicit aliasing of Get- commands for the CommandType ExternalScript (#1386) (by @bergmeister)

### Build/Tests/Maintenance

- Changelog for 1.19.0 (#1448) (by @bergmeister)
- Use separate test.yaml files files since param passing of pwsh still doesn't seem to fully work correctly #1466 (by @bergmeister)
- New build scripts for release pipeline (#1442) (by @JamesWTruher)
- Make sure that tests always get uploaded, due to too much YAML indentation, condition was not applied (#1455) (by @bergmeister)
- Cleanup csproj files: Remove code duplication for Configuration and entries in Rules project, since it depends on Engine project (#1450) (by @bergmeister)
- Build: Use logging command as a temporary workaround due to billing bug preventing upload of pipeline artefacts (#1464) (by @bergmeister)
- Use bracket syntax for passing yaml parameter (#1449) (by @bergmeister)
- Update Newtonsoft.Json from 12.0.2 to 12.0.3 in Compatibility project as well (#1447) (by @bergmeister)
- Replace usage of $MyInvocation.MyCommand.Path with $PSScriptRoot and cleanup tests as a preparation for Pester v5 (#1438) (by @bergmeister)
- Compatibility tests: Use correct genericVerCases test case and remove invalid test case (#1440) (by @bergmeister)
- Add out folder to .gitignore (#1443) (by @bergmeister)
- Pin major version of Pester to 4 and upgrade Pester version for WMF4 build (#1433) (by @bergmeister)
- ms-vscode.csharp to ms-dotnettools.csharp (#1424) (thanks @devlead!)
- Upgrade .net sdk to latest path (#1421) (by @bergmeister)
- Fix #1417 modulehelp false positives (#1418) (thanks @Jawz84!)
- Increase macOs CI coverage to include both 10.14 and 10.15 as 10.15 has recently been added to Azure DevOps hosted agents (#1412) (by @bergmeister)
- Fix vmImage entries in build to match description (#1409) (by @bergmeister)
- Update .Net Core SDK from 3.1.100 to latest patch 3.1.101 (#1405) (by @bergmeister)
- Multi-stage pipelines (to split build->test) (#1404) (by @bergmeister)
- Use vmImage in yaml and merge build definitions into 1 by using jobs. Disable all AppVeyor images except for WMF4. Disable flaky UseCompatibleCommand tests on Ubuntu in CI (#1403) (by @bergmeister)
- Azure pipelines migration (#1267) (by @bergmeister)
- Add DevContainer support (#1286) (by @bergmeister)
- Increase maximum runspace pool size in flaky test (#1358) (by @bergmeister)
- Use latest version of PlatyPS in AppVeyor (#1390) (by @bergmeister)
- .Net core sdk 3.1 (#1388) (by @bergmeister)
- Use Where-Object instead of alias in buildmodule.tests.ps1 (#1360) (thanks @kvprasoon!)
- Remove legacy methods (#1387) (by @bergmeister)
- Upgrade .Net Core SDK version to a newer version that is also available in Visual Studio Online environment (#1377) (by @bergmeister)
- Update Newtonsoft.Json from 12.0.1 to 12.02 (latest version), which increases the minimum PS Core version from 6.2.0 to 6.2.1 (#1357) (by @bergmeister)
- Harden dotnet --version handling (#1367) (by @JamesWTruher)
- Fix configuration for PS v5 and v6 builds to be Debug/Release as it used to be to allow for actual debug builds (#1356) (by @bergmeister)
- Disable bad tests on Linux for now (#1365) (by @rjmholt)
- Fix Ubuntu build and runspace test (#1340) (by @bergmeister)

### Documentation

- Update Readme with minimum PowerShell Core version being 6.2 (#1457) (by @bergmeister)
- Change term to satisfy policheck requirements #1445 (by @JamesWTruher)
- Update adopters file with entry for Microsoft internal teams (#1398) (thanks @pperrier27!)
- Add ADOPTERS.md (#1380) (by @SteveL-MSFT)
- Update SuppressMessageAttribute docs link and use single quotes for examples (#1389) (by @bergmeister)
- Update ShouldProcess.md (#1383) (thanks @baardhermansen!)
- Fix typo in example (#1379) (thanks @Farwaykorse!)
- Update Hyperlink for Approved Verbs (#1374) (thanks @MysticRyuujin!)

## [1.18.3](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.18.3) - 2019-09-13

This release adds one more important fix, which prevents a leak of runspaces when repeatedly invoking the PSScriptAnalyzer cmdlets. Furthermore it ups the minimum version of PowerShell Core to `6.2.0` due to `6.1` reaching its [end of life](https://learn.microsoft.com/powershell/scripting/install/PowerShell-Support-Lifecycle#powershell-end-of-support-dates) this month.

- Change CommandInfoCache to implement IDisposable and clean up the runspace pool (#1335) (by @JamesWTruher)
- Update Newtonsoft json to 12.0.1 due to PowerShell 6.1 going out of support (#1336) (by @bergmeister)

## [1.18.2](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.18.2) - 2019-09-03

This release mainly brings fixes and enhancements that are especially useful when being consumed by PowerShellEditorServices.
The benefit to the user will be some enhancements and fixes in the formatter, especially around multiline commands. In addition to that, error messages emitted when using invalid syntax in the setting file are now much more meaningful and actionable.

### Fixes

- Compatibility Analysis
  - Fix profile collection on non-Windows, add PS 7 profiles (#1260) (by @rjmholt)

- Formatter
  - Fix PSCloseBrace rule to not wrongly flag closing brace of one-line hashtable, which lead to incorrect formatting (#1309) (by @bergmeister)
  - PipelineIndentationStyle (when not being set to NoIndentation)
    - PipelineIndentationStyle: Fix edge case where pipeline was incorrectly detected to span multiple lines due to backticks in the command leading up to the pipeline (#1312) (by @bergmeister)
    - Fix edge case when PipelineIndentationStyle is not set to NoIndentation (pipeline operator spanning only one line but PipelineAst spanning multiple lines) (#1261) (by @bergmeister)
  - Allow formatter to optionally correct aliases (#1277) (by @bergmeister)

- Engine
  - Work around runspacepool deadlock (#1316) (by @rjmholt)

### Enhancements

- Formatter
  - Fix Indentation of multiline command with backticks after comment line (#1318) (by @bergmeister)
- Code Analysis:
  - Enhance UseDeclaredVarsMoreThanAssignments to detect also take into account the usage of Get-Variable with an array of variables and usage of named - parameter -Name (#1310) (by @bergmeister)
- Settings files
  - Actionable errors from incorrect settings files (#1263) (thanks @travis-c-lagrone!)
  - Update engine resource string 'WrongKeyHashTable' to include all valid settings hashtable keys (#1273) (thanks @travis-c-lagrone!)
- Add icon and add reference in readme and manifest (#1291) (by @bergmeister, thanks @adilio for creating the logo!)

### Documentation

- Update readme with changes to default branch being master now (#1265) (by @bergmeister)
- UseApprovedVerbs.md: Improving Documentation (#1294) (Thanks @Banner-Keith!)
- Update documentation for parameter "Settings" of command "Invoke-ScriptAnalyzer" to include the key "RecurseCustomRulesPath" - (#1272) (thanks @travis-c-lagrone!)

### Build

- Don't sign the compatability collector catalog so that PSScriptAnalyzer can ship with a catalog file again (#1328) (by @JamesWTruher)
- Fix Ubuntu Appveyor build (#1320) (by @bergmeister, thanks @JamesWTruher for the initial investigation)

## [1.18.1](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.18.1) - 2019-06-12

### Fixes

- Formatter
  - UseCorrectCasing
    - Do not use alias name as key for command info cache to fix the problem where UseCorrectCasing corrects aliases (#1216) (by @bergmeister)
    - Fix wildcard lookups for command lookup and do not append .exe for applications on Windows (#1210) (by @bergmeister)
    - Do not correct applications or script paths at all (#1255) (by @bergmeister)
  - Prevent PSCloseBrace crash if hashtable definition start on first token and there is a PSCloseBrace violation (#1235) (by @bergmeister)
  - PipelineIndentationStyle
    - Fix formatting regression in 1.18.0 whereby single-line pipeline reduces indentation level incorrectly (#1191) (by @bergmeister)
    - Make IncreaseIndentationForFirstPipeline the true default option for PipelineIndentationStyle option (#1218) (by @bergmeister)

- Compatibility Rules
  - Fix UseCompatibleSyntax class error message (#1215) (by @rjmholt)
  - Convert compatibility module to binary module, fix compatibility with Azure environments (#1212) (by @rjmholt)
  - Prevent .NET members with names differing only by case from crashing the compatibility profiler (#1195) (by @rjmholt)
  - Fix compatibility profile query API so that aliases referring to other modules appear (#1194) (by @rjmholt)

- DSC
  - Instead of using the first cimClass and then having no superClass, use the first cimClass that has a non-null superClass (#1200) (by @bergmeister, thanks to @ykuijs! for the great collaboration)
  - Make `-SaveDscDependency` work on Linux (#1246) (by @bergmeister)

- Enable suppression of custom rules when used together with `-IncludeDefaultRules` to allow all possible scenarios from 1.17.1 and 1.18.0. This removes also the check if the rule name in the suppression attribute can be found because this check is technically not possible in all scenarios (#1245) (by @bergmeister)
- Fix NullReferenceException for class type (#1182) (by @bergmeister)

### Performance

2 improvements were made that make cold runs of ScriptAnalyzer twice as fast compared to `1.18.0`

- Speedup cold runs by using a runspace pool (#1178) (by @bergmeister)
- Fix logic errors in AvoidAlias rule to halve the amount of necessary calls to `Get-Command`, making the rule twice (#1251) (by @JamesWTruher)

### General Improvements

- Add ImplementingType to RuleInfo object for better settings file validation and settings template file creation (#1250) (by @JamesWTruher)
- Fix typo in AvoidDefaultValueForMandatoryParameterError resource string (#1233) (Thanks @tnieto88!)
- Change module root find function in CompatibilityRule (#1196) (by @rjmholt)

### Documentation

- Update PossibleIncorrectComparisonWithNull documentation with better example (#1244, #1226 and #1220) (by @bergmeister and thanks to @PrzemyslawKlys and @lesterw1!)
- Compatibility rules documentation fixes (#1229, #1204, #1225 and #1221) (by @rjmholt, @bergmeister and thanks @skycommand!)
- Update README.md on supported PowerShell Core version and update Docker examples (#1188) (Thanks @PrzemyslawKlys!)
- Update UseConsistentWhitespace documentation to show all options (#1185) (by @bergmeister)

### Build

- Make it possible to build ScriptAnalyzer with PowerShell7 (#1256) (by @bergmeister)
- Update signing file for versioned location of module (#1253) (by @JamesWTruher)
- Bump version to 1.18.1 (#1248) (by @bergmeister)
- Change build script to install into a versioned directory (#1247) (by @JamesWTruher)
- Remove code for deprecated rules that is not used any more (#1232) (by @bergmeister)
- Speedup CI builds by skipping the first time initialization of the bootstrapped dotnet CLI (#1211) (by @bergmeister)
- Add suppression of warning for MS policy (#1205) (by @JamesWTruher)
- Change logic for copying profiles into the 'install' directory (#1197) (by @JamesWTruher)

## [1.18.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.18.0) - 2019-03-21

### Breaking Changes

- Minimum required version of `PowerShell Core` was raised from `6.0.2` to `6.1.0` due to `6.0` having reached the end of its support life-cycle. (#1165)
- Updated used version of `Newtonsoft.Json` to `11.0.2`. (#1165) (by @bergmeister)
- Command data files of PowerShell `6.0` were removed. Updated files for PowerShell `6.1` were added. This means that users that have customized the `UseCompatibleCmdlets` rule, need to either adapt the version string as per rule documentation or can alternatively copy in the old command data files into the `Settings` installation folder (#1075) (by @bergmeister)
- The minimum required runtime of the full .Net Framework was raised from `4.5.1` to `4.5.2` as `4.5.1` is not supported any more. If the OS is patched then Windows update will have ensured that the newer runtime patch is available. (#1108) (by @bergmeister)
- Emit parsing errors as diagnostic records (#1130) (by @JamesWTruher)

### Formatter Rules

- Add `UseCorrectCasing` formatting rule for cmdlet/function name (#1117) (by @bergmeister)
- Fix `PSUseConsistentIndentationRule` to handle pipes correctly when there is a multi-line statement after a pipe and add `PipelineIndentation` customisation option for it (#1102) (by @bergmeister)
- Add `CheckInnerBrace` and `CheckPipe` options to `PSUseConsistentWhitespace` (#1092) (by @bergmeister)

### Script Analysis Rules

- Add command data file for PowerShell `2.0` (#1091) (by @bergmeister)
- Add command data files of PowerShell `6.1.0` (#1075) (by @bergmeister)
- New PowerShell compatibility rules: `UseCompatibleCommands`, `UseCompatibleSyntax` and `UseCompatibleTypes` (#1156, #1133, #1176 and #1179) (by @rjmholt and @JamesWTruher)
- Make `PossibleIncorrectComparisonWithNull` rule return a `SuggestCorrection` for auto-fixes in VS-Code or via the -Fix switch (#1115) (by @bergmeister)
- Make UseCompatibleCmdlets not throw if default reference desktop-5.1.14393.206-windows is specified in the list of platforms and use core-6.1.0-windows as an alternative default reference (#1043 and #1075) (by @bergmeister)

### Custom Rules

- Translate `RuleSuppressionID` for custom rules as well (#1144) (by @bergmeister)
- Allow for suppression of custom rules (#1145) (by @bergmeister)

### DSC

- Add support for Hashtable syntax of `Import-DscRessource` for `-SaveDscDependency` switch (#1138) (by @bergmeister)
- Take module version into account for `-SaveDscDependency` switch (#1094) (by @bergmeister)

### Performance

Multi-threading efficiency was highly improved leading to a speedup whilst keeping the CPU usage roughly the same. One can expect `PSScriptAnalyzer` (PSSA) 1.18.0 to be more than twice as fast in most situations (including 'cold' runs) compared to its predecessor 1.17.0.

- Increase lock granularity for CommandInfo cache #1166 (by @bergmeister and @rjmholt)
- Restore caching in Helper.GetCommandInfo (#1074) (Thanks @SeeminglyScience!)

### Fixes and General Improvements

- Fix bug when culture was set to Turkish (#1095) (by @bergmeister)
- Fix settings file array parsing when no commas are present (#1161) (by @rjmholt)
- Ensure correct disposal with using statement (#1164) (by @bergmeister)
- Fix bug that caused Invoke-ScriptAnalyzer to not run any rule after an execution of Invoke-Formatter in the same session (but Invoke-ScriptAnalyzer would've recovered when being called again) (#1121) (by @bergmeister)
- Fix parsing of empty hashtable when string is provided as settings object (#1073) (by @bergmeister)
- Fix AlignAssignment rule to calculate alignment position correctly and avoid crash (#1070) (by @bergmeister)
- Don't crash on CIM classes with no superclass (#1046) (Thanks @edyoung!)
- Trigger AvoidPositionalParameters rule for function defined and called inside a script. (#963 and #1175) (Thanks @kalgiz! and @bergmeister)
- Add OutputType attributes to cmdlets to enhance tab completion (#1020) (by @bergmeister)
- Allow paths to be pipelined to Invoke-ScriptAnalyzer (#1040) (Thanks @edyoung!)
- PSGallery will no longer use PSAvoidUsingConvertToSecureStringWithPlainText and PSAvoidUsingUserNameAndPasswordParams rules (#1037) (Thanks @edyoung!)

### Build

- Update platyps to 0.13 (#1172) (by @bergmeister)
- Raise minimum required PS version of PS 6 to `6.1.0` due to 6.0 running out of support now and update Newtonsoft.Json to `11.0.2` (#1165) (by @bergmeister)
- Unify reference to `Microsoft.Management.Infrastructure` and update `System.Reflection.TypeExtensions` NuGet package (#1105) (by @bergmeister)
- Update .Net Core SDK from 2.2.103 to `2.2.104` (latest patch) (#1158) (by @bergmeister)
- Make it easier to install the dotnet CLI tools (#1139) (by @JamesWTruher)
- Simplify build scripts even more and upgrade platyPS in Appveyor #1088 (by @bergmeister)
- Upgrade .Net SDK to 2.2.102 to fix new test failures on AppVeyor's Ubuntu image due an image update (#1128) (by @bergmeister)
- Mark setting files as content for easier out-of-the-box debugging (#1104) (by @bergmeister)
- Upgrade from net451 to net452 since net451 is not supported any more and net452 is an in-place upgrade anyway (#1108) (by @bergmeister)
- Install dotnet sdk if missing on Unix as well to fix failing Ubuntu build due to image update and Pester/PowerShell bug (#1107) (by @bergmeister)
- Removes update of pwsh since appveyor images are updated (#1084) (Thanks @wilmardo!)
- Update, simplify, and consolidate build scripts (#1082) (by @JamesWTruher)
- Bump .Net Core SDK version to 2.1.401 (#1086) (Thanks @wilmardo!)
- Use RTM version of `Microsoft.Management.Infrastructure` (#1059) (by @bergmeister)
- Upgrade `Microsoft.PowerShell.5.ReferenceAssemblies` from 1.0.0 to `1.1.0` and remove redundant/transitive references (#1050) (by @bergmeister)
- Remove RuntimeFrameworkVersion as this is given by the running version of PS anyway and .Net would upgrade automatically anyway if possible (#1052) (by @bergmeister)
- Upgrade Nuget packages (`System.Reflection.TypeExtensions` and `Microsoft.CSharp`) and make them more consistent (#1051) (by @bergmeister)
- give explicit error on unsupported versions of pscore and document it (#1049) (by @bergmeister)
- Remove old and redundant test settings file core-6.0.0-alpha-windows.json (#1044) (by @bergmeister)
- Use `TLS 1.2` for bootstrapping dotnet in CI (#1047) (by @bergmeister)
- Remove redundant/outdated build scripts and cleanup (#1011) (by @bergmeister)
- Upload only out folder as artifact, which is the actual PSSA artifact (#1009) (by @bergmeister)

### Documentation

- Fix the "How" section #1127 (Thanks @alexandair!)
- Make the correct example use the declared variable (#1111) (Thanks @f0nt4!)
- Correcting CommunityRules to `CommunityAnalyzerRules` (#1119) (Thanks @kvprasoon!)
- Add summary comment to some rules with short description (#1087) (Thanks @pedrocodacy!)
- Update PR template to be more similar to the one of PowerShell but still distinguish where necessary (#1077)
- Update documentation of `PSUseConsistentWhiteSpace`: enhance example and correct argument type. #1069 (Thanks @TheIncorrigible1!)
- Update AvoidNullOrEmptyHelpMessageAttribute.md fix typo (#1068) (Thanks @TheIncorrigible1!)

## [1.17.1](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.17.1) - 2018-06-06

### Fixes

- Fix signing so `PSScriptAnalyzer` can be installed without the `-SkipPublisherCheck` switch (#1014)
- Issues with rule `PSAvoidAssignmentToAutomaticVariable` were fixed (#1007, #1013, #1014)
- Rule documentation update and cleanup (#988)

## [1.17.0](https://github.com/PowerShell/PSScriptAnalyzer/tree/1.17.0) - 2018-06-02

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
- Allowlist aliases comparison in AvoidUsingCmdletAliases rule (#739).
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
- Allowlist `Data` in `PSUseSingularNoun` rule
- Fix rule documentation of `PSDSCExamplesPresent`
- Fix false positives caused by PSD1 files which are not module manifests
    - affects `PSUseToExportFieldsInManifest`, `PSMissingModuleManifestField` and `PSAvoidUsingDeprecatedManifestFields` rules
- Use PlatyPS for generating MAML based documentation
- Add build script to automate building and testing the solution

A big **Thank You!** to the following folks for making PSScriptAnalyzer even better:
- [Kieran Jacobsen (@kjacobsen)](https://github.com/PowerShell/PSScriptAnalyzer/commits/development?author=kjacobsen): Fix rule documentation of `PSDSCExamplesPresent` [PR #591](#591)
- [Charlie Schmidt (@charlieschmidt)](https://github.com/PowerShell/PSScriptAnalyzer/commits/development?author=charlieschmidt): Suppress External Rules [PR #585](#585)
- [June Blender (@juneb)](https://github.com/PowerShell/PSScriptAnalyzer/commits/development?author=juneb): Add tests for module help [058f65e1](https://github.com/PowerShell/PSScriptAnalyzer/commit/058f65e1f6278222378fedf444eecb2e32865b1e)
- [Shayde Nofziger (@Blackbaud-ShaydeNofziger)](https://github.com/PowerShell/PSScriptAnalyzer/commits/development?author=Blackbaud-ShaydeNofziger): Fix rule name typo and comment [PR #560](#560)

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
- Fixed an issue where ScriptAnalyzer would become non-responsive when recursively analyzing a large data set
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
- CustomizedRulePath is now called CustomRulePath
- Fixes to handle folder paths
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
- Rule & Cmdlet documentation updates
- Cmdlet help file addition

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
- Removed PSAvoidTrapStatement rule from the builtin set since this is not deprecated and using trap is a better choice in certain scenarios.

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
