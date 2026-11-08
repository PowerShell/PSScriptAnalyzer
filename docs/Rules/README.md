---
description: List of PSScriptAnalyzer rules
ms.date: 07/24/2026
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
  [Using PSScriptAnalyzer][suppressing-rules].

|                        Rule                        |  Severity   | Default state  | Configurable |
| -------------------------------------------------- | ----------- | :------------: | :----------: |
| [AlignAssignmentStatement][]                       | Warning     |    Disabled    |     Yes      |
| [AvoidAssignmentToAutomaticVariable][]             | Warning     | Always enabled |              |
| [AvoidDefaultValueForMandatoryParameter][]         | Warning     | Always enabled |              |
| [AvoidDefaultValueSwitchParameter][]               | Warning     | Always enabled |              |
| [AvoidDynamicallyCreatingVariableNames][]          | Information |    Disabled    |     Yes      |
| [AvoidExclaimOperator][]                           | Warning     |    Disabled    |     Yes      |
| [AvoidGlobalAliases][]                             | Warning     | Always enabled |              |
| [AvoidGlobalFunctions][]                           | Warning     | Always enabled |              |
| [AvoidGlobalVars][]                                | Warning     | Always enabled |              |
| [AvoidInvokingEmptyMembers][]                      | Warning     | Always enabled |              |
| [AvoidLongLines][]                                 | Warning     |    Disabled    |     Yes      |
| [AvoidMultipleTypeAttributes][]                    | Warning     | Always enabled |              |
| [AvoidNullOrEmptyHelpMessageAttribute][]           | Warning     | Always enabled |              |
| [AvoidOverwritingBuiltInCmdlets][]                 | Warning     |    Enabled     |     Yes      |
| [AvoidReservedWordsAsFunctionNames][]              | Warning     | Always enabled |              |
| [AvoidSemicolonsAsLineTerminators][]               | Warning     |    Disabled    |     Yes      |
| [AvoidShouldContinueWithoutForce][]                | Warning     | Always enabled |              |
| [AvoidTrailingWhitespace][]                        | Information | Always enabled |              |
| [AvoidUsingAllowUnencryptedAuthentication][]       | Warning     | Always enabled |              |
| [AvoidUsingArrayList][]                            | Warning     |    Disabled    |     Yes      |
| [AvoidUsingBrokenHashAlgorithms][]                 | Warning     | Always enabled |              |
| [AvoidUsingCmdletAliases][]                        | Warning     | Always enabled |     Yes      |
| [AvoidUsingComputerNameHardcoded][]                | Error       | Always enabled |              |
| [AvoidUsingConvertToSecureStringWithPlainText][]   | Error       | Always enabled |              |
| [AvoidUsingDeprecatedManifestFields][]             | Warning     | Always enabled |              |
| [AvoidUsingDoubleQuotesForConstantString][]        | Information |    Disabled    |     Yes      |
| [AvoidUsingEmptyCatchBlock][]                      | Warning     | Always enabled |              |
| [AvoidUsingInvokeExpression][]                     | Warning     | Always enabled |              |
| [AvoidUsingPlainTextForPassword][]                 | Warning     | Always enabled |              |
| [AvoidUsingPositionalParameters][]                 | Information |    Enabled     |     Yes      |
| [AvoidUsingUsernameAndPasswordParams][]            | Error       | Always enabled |              |
| [AvoidUsingWMICmdlet][]                            | Warning     | Always enabled |              |
| [AvoidUsingWriteHost][]                            | Warning     | Always enabled |              |
| [DSCDscExamplesPresent][]                          | Information | Always enabled |              |
| [DSCDscTestsPresent][]                             | Information | Always enabled |              |
| [DSCReturnCorrectTypesForDSCFunctions][]           | Information | Always enabled |              |
| [DSCStandardDSCFunctionsInResource][]              | Error       | Always enabled |              |
| [DSCUseIdenticalMandatoryParametersForDSC][]       | Error       | Always enabled |              |
| [DSCUseIdenticalParametersForDSC][]                | Error       | Always enabled |              |
| [DSCUseVerboseMessageInDSCResource][]              | Information | Always enabled |              |
| [InvalidMultiDotValue][]                           | Warning     |    Disabled    |     Yes      |
| [MisleadingBacktick][]                             | Warning     | Always enabled |              |
| [MissingModuleManifestField][]                     | Warning     | Always enabled |              |
| [MissingTryBlock][]                                | Warning     |    Disabled    |     Yes      |
| [PlaceCloseBrace][]                                | Warning     |    Disabled    |     Yes      |
| [PlaceOpenBrace][]                                 | Warning     |    Disabled    |     Yes      |
| [PossibleIncorrectComparisonWithNull][]            | Warning     | Always enabled |              |
| [PossibleIncorrectUsageOfAssignmentOperator][]     | Information | Always enabled |              |
| [PossibleIncorrectUsageOfRedirectionOperator][]    | Warning     | Always enabled |              |
| [ProvideCommentHelp][]                             | Information |    Enabled     |     Yes      |
| [ReservedCmdletChar][]                             | Warning     | Always enabled |              |
| [ReservedParams][]                                 | Error       | Always enabled |              |
| [ReviewUnusedParameter][]                          | Warning     | Always enabled |     Yes      |
| [ShouldProcess][]                                  | Warning     | Always enabled |              |
| [UseApprovedVerbs][]                               | Warning     | Always enabled |              |
| [UseBOMForUnicodeEncodedFile][]                    | Warning     | Always enabled |              |
| [UseCmdletCorrectly][]                             | Warning     | Always enabled |              |
| [UseCompatibleCmdlets][]                           | Warning     | Always enabled |     Yes      |
| [UseCompatibleCommands][]                          | Warning     |    Disabled    |     Yes      |
| [UseCompatibleSyntax][]                            | Error       |    Disabled    |     Yes      |
| [UseCompatibleTypes][]                             | Warning     |    Disabled    |     Yes      |
| [UseConsistentIndentation][]                       | Warning     |    Disabled    |     Yes      |
| [UseConsistentParameterSetName][]                  | Warning     |    Disabled    |     Yes      |
| [UseConsistentParametersKind][]                    | Warning     |    Disabled    |     Yes      |
| [UseConsistentWhitespace][]                        | Warning     |    Disabled    |     Yes      |
| [UseConstrainedLanguageMode][]                     | Warning     |    Disabled    |     Yes      |
| [UseCorrectCasing][]                               | Information |    Disabled    |     Yes      |
| [UseDeclaredVarsMoreThanAssignments][]             | Warning     | Always enabled |              |
| [UseLiteralInitializerForHashtable][]              | Warning     | Always enabled |              |
| [UseOutputTypeCorrectly][]                         | Information | Always enabled |              |
| [UseProcessBlockForPipelineCommand][]              | Warning     | Always enabled |              |
| [UsePSCredentialType][]                            | Warning     | Always enabled |              |
| [UseShouldProcessForStateChangingFunctions][]      | Warning     | Always enabled |              |
| [UseSingleValueFromPipelineParameter][]            | Warning     |    Disabled    |     Yes      |
| [UseSingularNouns][]                               | Warning     |    Enabled     |     Yes      |
| [UseSupportsShouldProcess][]                       | Warning     | Always enabled |              |
| [UseToExportFieldsInManifest][]                    | Warning     | Always enabled |              |
| [UseUsingScopeModifierInNewRunspaces][]            | Warning     | Always enabled |              |
| [UseUTF8EncodingForHelpFile][]                     | Warning     | Always enabled |              |

<!-- link references -->
[suppressing-rules]: ../using-scriptanalyzer.md#suppressing-rules
[AlignAssignmentStatement]: AlignAssignmentStatement.md
[AvoidAssignmentToAutomaticVariable]: AvoidAssignmentToAutomaticVariable.md
[AvoidDefaultValueForMandatoryParameter]: AvoidDefaultValueForMandatoryParameter.md
[AvoidDefaultValueSwitchParameter]: AvoidDefaultValueSwitchParameter.md
[AvoidDynamicallyCreatingVariableNames]: AvoidDynamicallyCreatingVariableNames.md
[AvoidExclaimOperator]: AvoidExclaimOperator.md
[AvoidGlobalAliases]: AvoidGlobalAliases.md
[AvoidGlobalFunctions]: AvoidGlobalFunctions.md
[AvoidGlobalVars]: AvoidGlobalVars.md
[AvoidInvokingEmptyMembers]: AvoidInvokingEmptyMembers.md
[AvoidLongLines]: AvoidLongLines.md
[AvoidMultipleTypeAttributes]: AvoidMultipleTypeAttributes.md
[AvoidNullOrEmptyHelpMessageAttribute]: AvoidNullOrEmptyHelpMessageAttribute.md
[AvoidOverwritingBuiltInCmdlets]: AvoidOverwritingBuiltInCmdlets.md
[AvoidReservedWordsAsFunctionNames]: AvoidReservedWordsAsFunctionNames.md
[AvoidSemicolonsAsLineTerminators]: AvoidSemicolonsAsLineTerminators.md
[AvoidShouldContinueWithoutForce]: AvoidShouldContinueWithoutForce.md
[AvoidTrailingWhitespace]: AvoidTrailingWhitespace.md
[AvoidUsingAllowUnencryptedAuthentication]: AvoidUsingAllowUnencryptedAuthentication.md
[AvoidUsingArrayList]: AvoidUsingArrayList.md
[AvoidUsingBrokenHashAlgorithms]: AvoidUsingBrokenHashAlgorithms.md
[AvoidUsingCmdletAliases]: AvoidUsingCmdletAliases.md
[AvoidUsingComputerNameHardcoded]: AvoidUsingComputerNameHardcoded.md
[AvoidUsingConvertToSecureStringWithPlainText]: AvoidUsingConvertToSecureStringWithPlainText.md
[AvoidUsingDeprecatedManifestFields]: AvoidUsingDeprecatedManifestFields.md
[AvoidUsingDoubleQuotesForConstantString]: AvoidUsingDoubleQuotesForConstantString.md
[AvoidUsingEmptyCatchBlock]: AvoidUsingEmptyCatchBlock.md
[AvoidUsingInvokeExpression]: AvoidUsingInvokeExpression.md
[AvoidUsingPlainTextForPassword]: AvoidUsingPlainTextForPassword.md
[AvoidUsingPositionalParameters]: AvoidUsingPositionalParameters.md
[AvoidUsingUsernameAndPasswordParams]: AvoidUsingUsernameAndPasswordParams.md
[AvoidUsingWMICmdlet]: AvoidUsingWMICmdlet.md
[AvoidUsingWriteHost]: AvoidUsingWriteHost.md
[DSCDscExamplesPresent]: DSCDscExamplesPresent.md
[DSCDscTestsPresent]: DSCDscTestsPresent.md
[DSCReturnCorrectTypesForDSCFunctions]: DSCReturnCorrectTypesForDSCFunctions.md
[DSCStandardDSCFunctionsInResource]: DSCStandardDSCFunctionsInResource.md
[DSCUseIdenticalMandatoryParametersForDSC]: DSCUseIdenticalMandatoryParametersForDSC.md
[DSCUseIdenticalParametersForDSC]: DSCUseIdenticalParametersForDSC.md
[DSCUseVerboseMessageInDSCResource]: DSCUseVerboseMessageInDSCResource.md
[InvalidMultiDotValue]: InvalidMultiDotValue.md
[MisleadingBacktick]: MisleadingBacktick.md
[MissingModuleManifestField]: MissingModuleManifestField.md
[MissingTryBlock]: MissingTryBlock.md
[PlaceCloseBrace]: PlaceCloseBrace.md
[PlaceOpenBrace]: PlaceOpenBrace.md
[PossibleIncorrectComparisonWithNull]: PossibleIncorrectComparisonWithNull.md
[PossibleIncorrectUsageOfAssignmentOperator]: PossibleIncorrectUsageOfAssignmentOperator.md
[PossibleIncorrectUsageOfRedirectionOperator]: PossibleIncorrectUsageOfRedirectionOperator.md
[ProvideCommentHelp]: ProvideCommentHelp.md
[ReservedCmdletChar]: ReservedCmdletChar.md
[ReservedParams]: ReservedParams.md
[ReviewUnusedParameter]: ReviewUnusedParameter.md
[ShouldProcess]: ShouldProcess.md
[UseApprovedVerbs]: UseApprovedVerbs.md
[UseBOMForUnicodeEncodedFile]: UseBOMForUnicodeEncodedFile.md
[UseCmdletCorrectly]: UseCmdletCorrectly.md
[UseCompatibleCmdlets]: UseCompatibleCmdlets.md
[UseCompatibleCommands]: UseCompatibleCommands.md
[UseCompatibleSyntax]: UseCompatibleSyntax.md
[UseCompatibleTypes]: UseCompatibleTypes.md
[UseConsistentIndentation]: UseConsistentIndentation.md
[UseConsistentParameterSetName]: UseConsistentParameterSetName.md
[UseConsistentParametersKind]: UseConsistentParametersKind.md
[UseConsistentWhitespace]: UseConsistentWhitespace.md
[UseConstrainedLanguageMode]: UseConstrainedLanguageMode.md
[UseCorrectCasing]: UseCorrectCasing.md
[UseDeclaredVarsMoreThanAssignments]: UseDeclaredVarsMoreThanAssignments.md
[UseLiteralInitializerForHashtable]: UseLiteralInitializerForHashtable.md
[UseOutputTypeCorrectly]: UseOutputTypeCorrectly.md
[UseProcessBlockForPipelineCommand]: UseProcessBlockForPipelineCommand.md
[UsePSCredentialType]: UsePSCredentialType.md
[UseShouldProcessForStateChangingFunctions]: UseShouldProcessForStateChangingFunctions.md
[UseSingleValueFromPipelineParameter]: UseSingleValueFromPipelineParameter.md
[UseSingularNouns]: UseSingularNouns.md
[UseSupportsShouldProcess]: UseSupportsShouldProcess.md
[UseToExportFieldsInManifest]: UseToExportFieldsInManifest.md
[UseUsingScopeModifierInNewRunspaces]: UseUsingScopeModifierInNewRunspaces.md
[UseUTF8EncodingForHelpFile]: UseUTF8EncodingForHelpFile.md
