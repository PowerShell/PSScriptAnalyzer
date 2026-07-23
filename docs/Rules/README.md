---
description: List of PSScriptAnalyzer rules
ms.date: 07/22/2026
ms.topic: reference
title: List of PSScriptAnalyzer rules
---
# PSScriptAnalyzer Rules

 The PSScriptAnalyzer module includes the following built-in rule definitions. For rules that
 support settings, you can enable or disable them by setting the `Enable` configuration property in
 a custom settings file. Rules listed as _Always enabled_ don't expose a per-rule `Enable` property.
 There are two ways to avoid using these rules:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of
  [Using PSScriptAnalyzer][01].

|                        Rule                        |  Severity   | Default state  | Configurable |
| -------------------------------------------------- | ----------- | :------------: | :----------: |
| [AlignAssignmentStatement][02]                     | Warning     |    Disabled    |     Yes      |
| [AvoidAssignmentToAutomaticVariable][03]           | Warning     | Always enabled |              |
| [AvoidDefaultValueForMandatoryParameter][04]       | Warning     | Always enabled |              |
| [AvoidDefaultValueSwitchParameter][05]             | Warning     | Always enabled |              |
| [AvoidDynamicallyCreatingVariableNames][06]        | Information |    Disabled    |     Yes      |
| [AvoidExclaimOperator][07]                         | Warning     |    Disabled    |     Yes      |
| [AvoidGlobalAliases][08]                           | Warning     | Always enabled |              |
| [AvoidGlobalFunctions][09]                         | Warning     | Always enabled |              |
| [AvoidGlobalVars][10]                              | Warning     | Always enabled |              |
| [AvoidInvokingEmptyMembers][11]                    | Warning     | Always enabled |              |
| [AvoidLongLines][12]                               | Warning     |    Disabled    |     Yes      |
| [AvoidMultipleTypeAttributes][13]                  | Warning     | Always enabled |              |
| [AvoidNullOrEmptyHelpMessageAttribute][14]         | Warning     | Always enabled |              |
| [AvoidOverwritingBuiltInCmdlets][15]               | Warning     |    Enabled     |     Yes      |
| [AvoidReservedWordsAsFunctionNames][16]            | Warning     | Always enabled |              |
| [AvoidSemicolonsAsLineTerminators][17]             | Warning     |    Disabled    |     Yes      |
| [AvoidShouldContinueWithoutForce][18]              | Warning     | Always enabled |              |
| [AvoidTrailingWhitespace][19]                      | Information | Always enabled |              |
| [AvoidUsingAllowUnencryptedAuthentication][20]     | Warning     | Always enabled |              |
| [AvoidUsingArrayList][21]                          | Warning     |    Disabled    |     Yes      |
| [AvoidUsingBrokenHashAlgorithms][22]               | Warning     | Always enabled |              |
| [AvoidUsingCmdletAliases][23]                      | Warning     | Always enabled |     Yes      |
| [AvoidUsingComputerNameHardcoded][24]              | Error       | Always enabled |              |
| [AvoidUsingConvertToSecureStringWithPlainText][25] | Error       | Always enabled |              |
| [AvoidUsingDeprecatedManifestFields][26]           | Warning     | Always enabled |              |
| [AvoidUsingDoubleQuotesForConstantString][27]      | Information |    Disabled    |     Yes      |
| [AvoidUsingEmptyCatchBlock][28]                    | Warning     | Always enabled |              |
| [AvoidUsingInvokeExpression][29]                   | Warning     | Always enabled |              |
| [AvoidUsingPlainTextForPassword][30]               | Warning     | Always enabled |              |
| [AvoidUsingPositionalParameters][31]               | Information | Always enabled |              |
| [AvoidUsingUsernameAndPasswordParams][32]          | Error       | Always enabled |              |
| [AvoidUsingWMICmdlet][33]                          | Warning     | Always enabled |              |
| [AvoidUsingWriteHost][34]                          | Warning     | Always enabled |              |
| [DSCDscExamplesPresent][35]                        | Information | Always enabled |              |
| [DSCDscTestsPresent][36]                           | Information | Always enabled |              |
| [DSCReturnCorrectTypesForDSCFunctions][37]         | Information | Always enabled |              |
| [DSCStandardDSCFunctionsInResource][38]            | Error       | Always enabled |              |
| [DSCUseIdenticalMandatoryParametersForDSC][39]     | Error       | Always enabled |              |
| [DSCUseIdenticalParametersForDSC][40]              | Error       | Always enabled |              |
| [DSCUseVerboseMessageInDSCResource][41]            | Information | Always enabled |              |
| [InvalidMultiDotValue][42]                         | Error       |    Disabled    |     Yes      |
| [MisleadingBacktick][43]                           | Warning     | Always enabled |              |
| [MissingModuleManifestField][44]                   | Warning     | Always enabled |              |
| [MissingTryBlock][45]                              | Warning     |    Disabled    |     Yes      |
| [PlaceCloseBrace][46]                              | Warning     |    Disabled    |     Yes      |
| [PlaceOpenBrace][47]                               | Warning     |    Disabled    |     Yes      |
| [PossibleIncorrectComparisonWithNull][48]          | Warning     | Always enabled |              |
| [PossibleIncorrectUsageOfAssignmentOperator][49]   | Information | Always enabled |              |
| [PossibleIncorrectUsageOfRedirectionOperator][50]  | Warning     | Always enabled |              |
| [ProvideCommentHelp][51]                           | Information |    Enabled     |     Yes      |
| [ReservedCmdletChar][52]                           | Warning     | Always enabled |              |
| [ReservedParams][53]                               | Error       | Always enabled |              |
| [ReviewUnusedParameter][54]                        | Warning     | Always enabled |     Yes      |
| [ShouldProcess][55]                                | Warning     | Always enabled |              |
| [UseApprovedVerbs][56]                             | Warning     | Always enabled |              |
| [UseBOMForUnicodeEncodedFile][57]                  | Warning     | Always enabled |              |
| [UseCmdletCorrectly][58]                           | Warning     | Always enabled |              |
| [UseCompatibleCmdlets][59]                         | Warning     | Always enabled |     Yes      |
| [UseCompatibleCommands][60]                        | Warning     |    Disabled    |     Yes      |
| [UseCompatibleSyntax][61]                          | Error       |    Disabled    |     Yes      |
| [UseCompatibleTypes][62]                           | Warning     |    Disabled    |     Yes      |
| [UseConsistentIndentation][63]                     | Warning     |    Disabled    |     Yes      |
| [UseConsistentParameterSetName][64]                | Warning     |    Disabled    |     Yes      |
| [UseConsistentParametersKind][65]                  | Warning     |    Disabled    |     Yes      |
| [UseConsistentWhitespace][66]                      | Warning     |    Disabled    |     Yes      |
| [UseConstrainedLanguageMode][67]                   | Warning     |    Disabled    |     Yes      |
| [UseCorrectCasing][68]                             | Information |    Disabled    |     Yes      |
| [UseDeclaredVarsMoreThanAssignments][69]           | Warning     | Always enabled |              |
| [UseLiteralInitializerForHashtable][70]            | Warning     | Always enabled |              |
| [UseOutputTypeCorrectly][71]                       | Information | Always enabled |              |
| [UseProcessBlockForPipelineCommand][72]            | Warning     | Always enabled |              |
| [UsePSCredentialType][73]                          | Warning     | Always enabled |              |
| [UseShouldProcessForStateChangingFunctions][74]    | Warning     | Always enabled |              |
| [UseSingleValueFromPipelineParameter][75]          | Warning     |    Disabled    |     Yes      |
| [UseSingularNouns][76]                             | Warning     |    Enabled     |     Yes      |
| [UseSupportsShouldProcess][77]                     | Warning     | Always enabled |              |
| [UseToExportFieldsInManifest][78]                  | Warning     | Always enabled |              |
| [UseUsingScopeModifierInNewRunspaces][79]          | Warning     | Always enabled |              |
| [UseUTF8EncodingForHelpFile][80]                   | Warning     | Always enabled |              |

<!-- link references -->
[01]: ../using-scriptanalyzer.md#suppressing-rules
[02]: AlignAssignmentStatement.md
[03]: AvoidAssignmentToAutomaticVariable.md
[04]: AvoidDefaultValueForMandatoryParameter.md
[05]: AvoidDefaultValueSwitchParameter.md
[06]: AvoidDynamicallyCreatingVariableNames.md
[07]: AvoidExclaimOperator.md
[08]: AvoidGlobalAliases.md
[09]: AvoidGlobalFunctions.md
[10]: AvoidGlobalVars.md
[11]: AvoidInvokingEmptyMembers.md
[12]: AvoidLongLines.md
[13]: AvoidMultipleTypeAttributes.md
[14]: AvoidNullOrEmptyHelpMessageAttribute.md
[15]: AvoidOverwritingBuiltInCmdlets.md
[16]: AvoidReservedWordsAsFunctionNames.md
[17]: AvoidSemicolonsAsLineTerminators.md
[18]: AvoidShouldContinueWithoutForce.md
[19]: AvoidTrailingWhitespace.md
[20]: AvoidUsingAllowUnencryptedAuthentication.md
[21]: AvoidUsingArrayList.md
[22]: AvoidUsingBrokenHashAlgorithms.md
[23]: AvoidUsingCmdletAliases.md
[24]: AvoidUsingComputerNameHardcoded.md
[25]: AvoidUsingConvertToSecureStringWithPlainText.md
[26]: AvoidUsingDeprecatedManifestFields.md
[27]: AvoidUsingDoubleQuotesForConstantString.md
[28]: AvoidUsingEmptyCatchBlock.md
[29]: AvoidUsingInvokeExpression.md
[30]: AvoidUsingPlainTextForPassword.md
[31]: AvoidUsingPositionalParameters.md
[32]: AvoidUsingUsernameAndPasswordParams.md
[33]: AvoidUsingWMICmdlet.md
[34]: AvoidUsingWriteHost.md
[35]: DSCDscExamplesPresent.md
[36]: DSCDscTestsPresent.md
[37]: DSCReturnCorrectTypesForDSCFunctions.md
[38]: DSCStandardDSCFunctionsInResource.md
[39]: DSCUseIdenticalMandatoryParametersForDSC.md
[40]: DSCUseIdenticalParametersForDSC.md
[41]: DSCUseVerboseMessageInDSCResource.md
[42]: InvalidMultiDotValue.md
[43]: MisleadingBacktick.md
[44]: MissingModuleManifestField.md
[45]: MissingTryBlock.md
[46]: PlaceCloseBrace.md
[47]: PlaceOpenBrace.md
[48]: PossibleIncorrectComparisonWithNull.md
[49]: PossibleIncorrectUsageOfAssignmentOperator.md
[50]: PossibleIncorrectUsageOfRedirectionOperator.md
[51]: ProvideCommentHelp.md
[52]: ReservedCmdletChar.md
[53]: ReservedParams.md
[54]: ReviewUnusedParameter.md
[55]: ShouldProcess.md
[56]: UseApprovedVerbs.md
[57]: UseBOMForUnicodeEncodedFile.md
[58]: UseCmdletCorrectly.md
[59]: UseCompatibleCmdlets.md
[60]: UseCompatibleCommands.md
[61]: UseCompatibleSyntax.md
[62]: UseCompatibleTypes.md
[63]: UseConsistentIndentation.md
[64]: UseConsistentParameterSetName.md
[65]: UseConsistentParametersKind.md
[66]: UseConsistentWhitespace.md
[67]: UseConstrainedLanguageMode.md
[68]: UseCorrectCasing.md
[69]: UseDeclaredVarsMoreThanAssignments.md
[70]: UseLiteralInitializerForHashtable.md
[71]: UseOutputTypeCorrectly.md
[72]: UseProcessBlockForPipelineCommand.md
[73]: UsePSCredentialType.md
[74]: UseShouldProcessForStateChangingFunctions.md
[75]: UseSingleValueFromPipelineParameter.md
[76]: UseSingularNouns.md
[77]: UseSupportsShouldProcess.md
[78]: UseToExportFieldsInManifest.md
[79]: UseUsingScopeModifierInNewRunspaces.md
[80]: UseUTF8EncodingForHelpFile.md
