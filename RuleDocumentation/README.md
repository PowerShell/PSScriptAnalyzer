# PSScriptAnalyzer Rules

## Table of Contents

|                                                 Rule                                                  |  Severity   | Enabled by default |  Configurable   |
| ----------------------------------------------------------------------------------------------------- | ----------- | :----------------: | :-------------: |
| [PSAlignAssignmentStatement](./PSAlignAssignmentStatement.md)                                         | Warning     |         No         |       Yes       |
| [PSAvoidAssignmentToAutomaticVariable](./PSAvoidAssignmentToAutomaticVariable.md)                     | Warning     |        Yes         |                 |
| [PSAvoidDefaultValueForMandatoryParameter](./PSAvoidDefaultValueForMandatoryParameter.md)             | Warning     |        Yes         |                 |
| [PSAvoidDefaultValueSwitchParameter](./PSAvoidDefaultValueSwitchParameter.md)                         | Warning     |        Yes         |                 |
| [PSAvoidGlobalAliases<sup>1</sup>](./PSAvoidGlobalAliases.md)                                         | Warning     |        Yes         |                 |
| [PSAvoidGlobalFunctions](./PSAvoidGlobalFunctions.md)                                                 | Warning     |        Yes         |                 |
| [PSAvoidGlobalVars](./PSAvoidGlobalVars.md)                                                           | Warning     |        Yes         |                 |
| [PSAvoidInvokingEmptyMembers](./PSAvoidInvokingEmptyMembers.md)                                       | Warning     |        Yes         |                 |
| [PSAvoidLongLines](./PSAvoidLongLines.md)                                                             | Warning     |         No         |       Yes       |
| [PSAvoidMultipleTypeAttributes<sup>1</sup>](./PSAvoidMultipleTypeAttributes.md)                       | Warning     |        Yes         |                 |
| [PSAvoidNullOrEmptyHelpMessageAttribute](./PSAvoidNullOrEmptyHelpMessageAttribute.md)                 | Warning     |        Yes         |                 |
| [PSAvoidOverwritingBuiltInCmdlets](./PSAvoidOverwritingBuiltInCmdlets.md)                             | Warning     |        Yes         |       Yes       |
| [PSAvoidShouldContinueWithoutForce](./PSAvoidShouldContinueWithoutForce.md)                           | Warning     |        Yes         |                 |
| [PSAvoidTrailingWhitespace](./PSAvoidTrailingWhitespace.md)                                           | Warning     |        Yes         |                 |
| [PSAvoidUsingCmdletAliases](./PSAvoidUsingCmdletAliases.md)                                           | Warning     |        Yes         | Yes<sup>2</sup> |
| [PSAvoidUsingComputerNameHardcoded](./PSAvoidUsingComputerNameHardcoded.md)                           | Error       |        Yes         |                 |
| [PSAvoidUsingConvertToSecureStringWithPlainText](./PSAvoidUsingConvertToSecureStringWithPlainText.md) | Error       |        Yes         |                 |
| [PSAvoidUsingDeprecatedManifestFields](./PSAvoidUsingDeprecatedManifestFields.md)                     | Warning     |        Yes         |                 |
| [PSAvoidUsingDoubleQuotesForConstantString](./PSAvoidUsingDoubleQuotesForConstantString.md)           | Warning     |         No         |       Yes       |
| [PSAvoidUsingEmptyCatchBlock](./PSAvoidUsingEmptyCatchBlock.md)                                       | Warning     |        Yes         |                 |
| [PSAvoidUsingInvokeExpression](./PSAvoidUsingInvokeExpression.md)                                     | Warning     |        Yes         |                 |
| [PSAvoidUsingPlainTextForPassword](./PSAvoidUsingPlainTextForPassword.md)                             | Warning     |        Yes         |                 |
| [PSAvoidUsingPositionalParameters](./PSAvoidUsingPositionalParameters.md)                             | Warning     |        Yes         |                 |
| [PSAvoidUsingUsernameAndPasswordParams](./PSAvoidUsingUsernameAndPasswordParams.md)                   | Error       |        Yes         |                 |
| [PSAvoidUsingWMICmdlet](./PSAvoidUsingWMICmdlet.md)                                                   | Warning     |        Yes         |                 |
| [PSAvoidUsingWriteHost](./PSAvoidUsingWriteHost.md)                                                   | Warning     |        Yes         |                 |
| [PSDSCDscExamplesPresent](./PSDSCDscExamplesPresent.md)                                               | Information |        Yes         |                 |
| [PSDSCDscTestsPresent](./PSDSCDscTestsPresent.md)                                                     | Information |        Yes         |                 |
| [PSDSCReturnCorrectTypesForDSCFunctions](./PSDSCReturnCorrectTypesForDSCFunctions.md)                 | Information |        Yes         |                 |
| [PSDSCStandardDSCFunctionsInResource](./PSDSCStandardDSCFunctionsInResource.md)                       | Error       |        Yes         |                 |
| [PSDSCUseIdenticalMandatoryParametersForDSC](./PSDSCUseIdenticalMandatoryParametersForDSC.md)         | Error       |        Yes         |                 |
| [PSDSCUseIdenticalParametersForDSC](./PSDSCUseIdenticalParametersForDSC.md)                           | Error       |        Yes         |                 |
| [PSDSCUseVerboseMessageInDSCResource](./PSDSCUseVerboseMessageInDSCResource.md)                       | Error       |        Yes         |                 |
| [PSMisleadingBacktick](./PSMisleadingBacktick.md)                                                     | Warning     |        Yes         |                 |
| [PSMissingModuleManifestField](./PSMissingModuleManifestField.md)                                     | Warning     |        Yes         |                 |
| [PSPlaceCloseBrace](./PSPlaceCloseBrace.md)                                                           | Warning     |         No         |       Yes       |
| [PSPlaceOpenBrace](./PSPlaceOpenBrace.md)                                                             | Warning     |         No         |       Yes       |
| [PSPossibleIncorrectComparisonWithNull](./PSPossibleIncorrectComparisonWithNull.md)                   | Warning     |        Yes         |                 |
| [PSPossibleIncorrectUsageOfAssignmentOperator](./PSPossibleIncorrectUsageOfAssignmentOperator.md)     | Warning     |        Yes         |                 |
| [PSPossibleIncorrectUsageOfRedirectionOperator](./PSPossibleIncorrectUsageOfRedirectionOperator.md)   | Warning     |        Yes         |                 |
| [PSProvideCommentHelp](./PSProvideCommentHelp.md)                                                     | Information |        Yes         |       Yes       |
| [PSReservedCmdletChar](./PSReservedCmdletChar.md)                                                     | Error       |        Yes         |                 |
| [PSReservedParams](./PSReservedParams.md)                                                             | Error       |        Yes         |                 |
| [PSReviewUnusedParameter](./PSReviewUnusedParameter.md)                                               | Warning     |        Yes         |                 |
| [PSShouldProcess](./PSShouldProcess.md)                                                               | Error       |        Yes         |                 |
| [PSUseApprovedVerbs](./PSUseApprovedVerbs.md)                                                         | Warning     |        Yes         |                 |
| [PSUseBOMForUnicodeEncodedFile](./PSUseBOMForUnicodeEncodedFile.md)                                   | Warning     |        Yes         |                 |
| [PSUseCmdletCorrectly](./PSUseCmdletCorrectly.md)                                                     | Warning     |        Yes         |                 |
| [PSUseCompatibleCmdlets](./PSUseCompatibleCmdlets.md)                                                 | Warning     |        Yes         | Yes<sup>2</sup> |
| [PSUseCompatibleCommands](./PSUseCompatibleCommands.md)                                               | Warning     |         No         |       Yes       |
| [PSUseCompatibleSyntax](./PSUseCompatibleSyntax.md)                                                   | Warning     |         No         |       Yes       |
| [PSUseCompatibleTypes](./PSUseCompatibleTypes.md)                                                     | Warning     |         No         |       Yes       |
| [PSUseConsistentIndentation](./PSUseConsistentIndentation.md)                                         | Warning     |         No         |       Yes       |
| [PSUseConsistentWhitespace](./PSUseConsistentWhitespace.md)                                           | Warning     |         No         |       Yes       |
| [PSUseCorrectCasing](./PSUseCorrectCasing.md)                                                         | Information |         No         |       Yes       |
| [PSUseDeclaredVarsMoreThanAssignments](./PSUseDeclaredVarsMoreThanAssignments.md)                     | Warning     |        Yes         |                 |
| [PSUseLiteralInitializerForHashtable](./PSUseLiteralInitializerForHashtable.md)                       | Warning     |        Yes         |                 |
| [PSUseOutputTypeCorrectly](./PSUseOutputTypeCorrectly.md)                                             | Information |        Yes         |                 |
| [PSUseProcessBlockForPipelineCommand](./PSUseProcessBlockForPipelineCommand.md)                       | Warning     |        Yes         |                 |
| [PSUsePSCredentialType](./PSUsePSCredentialType.md)                                                   | Warning     |        Yes         |                 |
| [PSUseShouldProcessForStateChangingFunctions](./PSUseShouldProcessForStateChangingFunctions.md)       | Warning     |        Yes         |                 |
| [PSUseSingularNouns<sup>1</sup>](./PSUseSingularNouns.md)                                             | Warning     |        Yes         |                 |
| [PSUseSupportsShouldProcess](./PSUseSupportsShouldProcess.md)                                         | Warning     |        Yes         |                 |
| [PSUseToExportFieldsInManifest](./PSUseToExportFieldsInManifest.md)                                   | Warning     |        Yes         |                 |
| [PSUseUsingScopeModifierInNewRunspaces](./PSUseUsingScopeModifierInNewRunspaces.md)                   | Warning     |        Yes         |                 |
| [PSUseUTF8EncodingForHelpFile](./PSUseUTF8EncodingForHelpFile.md)                                     | Warning     |        Yes         |                 |

- <sup>1</sup> Rule is not available on all PowerShell versions, editions, or OS platforms. See the
  rule's documentation for details.
- <sup>2</sup> The rule a configurable property, but the rule can't be disabled like other
  configurable rules.
