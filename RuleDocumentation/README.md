# PSScriptAnalyzer Rules

## Table of Contents

|                                               Rule                                                |  Severity   | Enabled by default |  Configurable   |
| ------------------------------------------------------------------------------------------------- | ----------- | :----------------: | :-------------: |
| [AlignAssignmentStatement](./AlignAssignmentStatement.md)                                         | Warning     |         No         |       Yes       |
| [AvoidAssignmentToAutomaticVariable](./AvoidAssignmentToAutomaticVariable.md)                     | Warning     |        Yes         |                 |
| [AvoidDefaultValueForMandatoryParameter](./AvoidDefaultValueForMandatoryParameter.md)             | Warning     |        Yes         |                 |
| [AvoidDefaultValueSwitchParameter](./AvoidDefaultValueSwitchParameter.md)                         | Warning     |        Yes         |                 |
| [AvoidGlobalAliases<sup>1</sup>](./AvoidGlobalAliases.md)                                         | Warning     |        Yes         |                 |
| [AvoidGlobalFunctions](./AvoidGlobalFunctions.md)                                                 | Warning     |        Yes         |                 |
| [AvoidGlobalVars](./AvoidGlobalVars.md)                                                           | Warning     |        Yes         |                 |
| [AvoidInvokingEmptyMembers](./AvoidInvokingEmptyMembers.md)                                       | Warning     |        Yes         |                 |
| [AvoidLongLines](./AvoidLongLines.md)                                                             | Warning     |         No         |       Yes       |
| [AvoidMultipleTypeAttributes<sup>1</sup>](./AvoidMultipleTypeAttributes.md)                       | Warning     |        Yes         |                 |
| [AvoidNullOrEmptyHelpMessageAttribute](./AvoidNullOrEmptyHelpMessageAttribute.md)                 | Warning     |        Yes         |                 |
| [AvoidOverwritingBuiltInCmdlets](./AvoidOverwritingBuiltInCmdlets.md)                             | Warning     |        Yes         |       Yes       |
| [AvoidShouldContinueWithoutForce](./AvoidShouldContinueWithoutForce.md)                           | Warning     |        Yes         |                 |
| [AvoidTrailingWhitespace](./AvoidTrailingWhitespace.md)                                           | Warning     |        Yes         |                 |
| [AvoidUsingCmdletAliases](./AvoidUsingCmdletAliases.md)                                           | Warning     |        Yes         | Yes<sup>2</sup> |
| [AvoidUsingComputerNameHardcoded](./AvoidUsingComputerNameHardcoded.md)                           | Error       |        Yes         |                 |
| [AvoidUsingConvertToSecureStringWithPlainText](./AvoidUsingConvertToSecureStringWithPlainText.md) | Error       |        Yes         |                 |
| [AvoidUsingDeprecatedManifestFields](./AvoidUsingDeprecatedManifestFields.md)                     | Warning     |        Yes         |                 |
| [AvoidUsingDoubleQuotesForConstantString](./AvoidUsingDoubleQuotesForConstantString.md)           | Warning     |         No         |       Yes       |
| [AvoidUsingEmptyCatchBlock](./AvoidUsingEmptyCatchBlock.md)                                       | Warning     |        Yes         |                 |
| [AvoidUsingInvokeExpression](./AvoidUsingInvokeExpression.md)                                     | Warning     |        Yes         |                 |
| [AvoidUsingPlainTextForPassword](./AvoidUsingPlainTextForPassword.md)                             | Warning     |        Yes         |                 |
| [AvoidUsingPositionalParameters](./AvoidUsingPositionalParameters.md)                             | Warning     |        Yes         |                 |
| [AvoidUsingUsernameAndPasswordParams](./AvoidUsingUsernameAndPasswordParams.md)                   | Error       |        Yes         |                 |
| [AvoidUsingWMICmdlet](./AvoidUsingWMICmdlet.md)                                                   | Warning     |        Yes         |                 |
| [AvoidUsingWriteHost](./AvoidUsingWriteHost.md)                                                   | Warning     |        Yes         |                 |
| [DSCDscExamplesPresent](./DSCDscExamplesPresent.md)                                               | Information |        Yes         |                 |
| [DSCDscTestsPresent](./DSCDscTestsPresent.md)                                                     | Information |        Yes         |                 |
| [DSCReturnCorrectTypesForDSCFunctions](./DSCReturnCorrectTypesForDSCFunctions.md)                 | Information |        Yes         |                 |
| [DSCStandardDSCFunctionsInResource](./DSCStandardDSCFunctionsInResource.md)                       | Error       |        Yes         |                 |
| [DSCUseIdenticalMandatoryParametersForDSC](./DSCUseIdenticalMandatoryParametersForDSC.md)         | Error       |        Yes         |                 |
| [DSCUseIdenticalParametersForDSC](./DSCUseIdenticalParametersForDSC.md)                           | Error       |        Yes         |                 |
| [DSCUseVerboseMessageInDSCResource](./DSCUseVerboseMessageInDSCResource.md)                       | Error       |        Yes         |                 |
| [MisleadingBacktick](./MisleadingBacktick.md)                                                     | Warning     |        Yes         |                 |
| [MissingModuleManifestField](./MissingModuleManifestField.md)                                     | Warning     |        Yes         |                 |
| [PlaceCloseBrace](./PlaceCloseBrace.md)                                                           | Warning     |         No         |       Yes       |
| [PlaceOpenBrace](./PlaceOpenBrace.md)                                                             | Warning     |         No         |       Yes       |
| [PossibleIncorrectComparisonWithNull](./PossibleIncorrectComparisonWithNull.md)                   | Warning     |        Yes         |                 |
| [PossibleIncorrectUsageOfAssignmentOperator](./PossibleIncorrectUsageOfAssignmentOperator.md)     | Warning     |        Yes         |                 |
| [PossibleIncorrectUsageOfRedirectionOperator](./PossibleIncorrectUsageOfRedirectionOperator.md)   | Warning     |        Yes         |                 |
| [ProvideCommentHelp](./ProvideCommentHelp.md)                                                     | Information |        Yes         |       Yes       |
| [ReservedCmdletChar](./ReservedCmdletChar.md)                                                     | Error       |        Yes         |                 |
| [ReservedParams](./ReservedParams.md)                                                             | Error       |        Yes         |                 |
| [ReviewUnusedParameter](./ReviewUnusedParameter.md)                                               | Warning     |        Yes         |                 |
| [ShouldProcess](./ShouldProcess.md)                                                               | Error       |        Yes         |                 |
| [UseApprovedVerbs](./UseApprovedVerbs.md)                                                         | Warning     |        Yes         |                 |
| [UseBOMForUnicodeEncodedFile](./UseBOMForUnicodeEncodedFile.md)                                   | Warning     |        Yes         |                 |
| [UseCmdletCorrectly](./UseCmdletCorrectly.md)                                                     | Warning     |        Yes         |                 |
| [UseCompatibleCmdlets](./UseCompatibleCmdlets.md)                                                 | Warning     |        Yes         | Yes<sup>2</sup> |
| [UseCompatibleCommands](./UseCompatibleCommands.md)                                               | Warning     |         No         |       Yes       |
| [UseCompatibleSyntax](./UseCompatibleSyntax.md)                                                   | Warning     |         No         |       Yes       |
| [UseCompatibleTypes](./UseCompatibleTypes.md)                                                     | Warning     |         No         |       Yes       |
| [UseConsistentIndentation](./UseConsistentIndentation.md)                                         | Warning     |         No         |       Yes       |
| [UseConsistentWhitespace](./UseConsistentWhitespace.md)                                           | Warning     |         No         |       Yes       |
| [UseCorrectCasing](./UseCorrectCasing.md)                                                         | Information |         No         |       Yes       |
| [UseDeclaredVarsMoreThanAssignments](./UseDeclaredVarsMoreThanAssignments.md)                     | Warning     |        Yes         |                 |
| [UseLiteralInitializerForHashtable](./UseLiteralInitializerForHashtable.md)                       | Warning     |        Yes         |                 |
| [UseOutputTypeCorrectly](./UseOutputTypeCorrectly.md)                                             | Information |        Yes         |                 |
| [UseProcessBlockForPipelineCommand](./UseProcessBlockForPipelineCommand.md)                       | Warning     |        Yes         |                 |
| [UsePSCredentialType](./UsePSCredentialType.md)                                                   | Warning     |        Yes         |                 |
| [UseShouldProcessForStateChangingFunctions](./UseShouldProcessForStateChangingFunctions.md)       | Warning     |        Yes         |                 |
| [UseSingularNouns<sup>1</sup>](./UseSingularNouns.md)                                             | Warning     |        Yes         |                 |
| [UseSupportsShouldProcess](./UseSupportsShouldProcess.md)                                         | Warning     |        Yes         |                 |
| [UseToExportFieldsInManifest](./UseToExportFieldsInManifest.md)                                   | Warning     |        Yes         |                 |
| [UseUsingScopeModifierInNewRunspaces](./UseUsingScopeModifierInNewRunspaces.md)                   | Warning     |        Yes         |                 |
| [UseUTF8EncodingForHelpFile](./UseUTF8EncodingForHelpFile.md)                                     | Warning     |        Yes         |                 |

- <sup>1</sup> Rule is not available on all PowerShell versions, editions, or OS platforms. See the
  rule's documentation for details.
- <sup>2</sup> The rule a configurable property, but the rule can't be disabled like other
  configurable rules.
