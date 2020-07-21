# PSScriptAnalyzer Rules

## Table of Contents

| Rule | Severity | Configurable |
|------|----------------------------------|--------------|
|[AlignAssignmentStatement](./AlignAssignmentStatement.md) | Warning | |
|[AvoidAssignmentToAutomaticVariable](./AvoidAssignmentToAutomaticVariable.md) | Warning | |
|[AvoidDefaultValueForMandatoryParameter](./AvoidDefaultValueForMandatoryParameter.md) | Warning | |
|[AvoidDefaultValueSwitchParameter](./AvoidDefaultValueSwitchParameter.md) | Warning | |
|[AvoidGlobalAliases<sup>*</sup>](./AvoidGlobalAliases.md) | Warning | |
|[AvoidGlobalFunctions](./AvoidGlobalFunctions.md) | Warning | |
|[AvoidGlobalVars](./AvoidGlobalVars.md) | Warning | |
|[AvoidInvokingEmptyMembers](./AvoidInvokingEmptyMembers.md) | Warning | |
|[AvoidLongLines](./AvoidLongLines.md) | Warning | |
|[AvoidOverwritingBuiltInCmdlets](./AvoidOverwritingBuiltInCmdlets.md) | Warning | |
|[AvoidNullOrEmptyHelpMessageAttribute](./AvoidNullOrEmptyHelpMessageAttribute.md) | Warning | |
|[AvoidShouldContinueWithoutForce](./AvoidShouldContinueWithoutForce.md) | Warning | |
|[UseUsingScopeModifierInNewRunspaces](./UseUsingScopeModifierInNewRunspaces.md) | Warning | |
|[AvoidUsingDoubleQuotesForConstantString](./AvoidUsingDoubleQuotesForConstantString.md) | Warning | Yes |
|[AvoidUsingCmdletAliases](./AvoidUsingCmdletAliases.md) | Warning | Yes |
|[AvoidUsingComputerNameHardcoded](./AvoidUsingComputerNameHardcoded.md) | Error | |
|[AvoidUsingConvertToSecureStringWithPlainText](./AvoidUsingConvertToSecureStringWithPlainText.md) | Error | |
|[AvoidUsingDeprecatedManifestFields](./AvoidUsingDeprecatedManifestFields.md) | Warning | |
|[AvoidUsingEmptyCatchBlock](./AvoidUsingEmptyCatchBlock.md) | Warning | |
|[AvoidUsingInvokeExpression](./AvoidUsingInvokeExpression.md) | Warning | |
|[AvoidUsingPlainTextForPassword](./AvoidUsingPlainTextForPassword.md) | Warning | |
|[AvoidUsingPositionalParameters](./AvoidUsingPositionalParameters.md) | Warning | |
|[AvoidTrailingWhitespace](./AvoidTrailingWhitespace.md) | Warning | |
|[AvoidUsingUsernameAndPasswordParams](./AvoidUsingUsernameAndPasswordParams.md) | Error | |
|[AvoidUsingWMICmdlet](./AvoidUsingWMICmdlet.md) | Warning | |
|[AvoidUsingWriteHost](./AvoidUsingWriteHost.md) | Warning | |
|[DSCDscExamplesPresent](./DSCDscExamplesPresent.md) | Information | |
|[DSCDscTestsPresent](./DSCDscTestsPresent.md) | Information | |
|[DSCReturnCorrectTypesForDSCFunctions](./DSCReturnCorrectTypesForDSCFunctions.md) | Information | |
|[DSCStandardDSCFunctionsInResource](./DSCStandardDSCFunctionsInResource.md) | Error | |
|[DSCUseIdenticalMandatoryParametersForDSC](./DSCUseIdenticalMandatoryParametersForDSC.md) | Error | |
|[DSCUseIdenticalParametersForDSC](./DSCUseIdenticalParametersForDSC.md) | Error | |
|[DSCUseVerboseMessageInDSCResource](./DSCUseVerboseMessageInDSCResource.md) | Error | |
|[MisleadingBacktick](./MisleadingBacktick.md) | Warning | |
|[MissingModuleManifestField](./MissingModuleManifestField.md) | Warning | |
|[PossibleIncorrectComparisonWithNull](./PossibleIncorrectComparisonWithNull.md) | Warning | |
|[PossibleIncorrectUsageOfAssignmentOperator](./PossibleIncorrectUsageOfAssignmentOperator.md) | Warning | |
|[PossibleIncorrectUsageOfRedirectionOperator](./PossibleIncorrectUsageOfRedirectionOperator.md) | Warning | |
|[ProvideCommentHelp](./ProvideCommentHelp.md) | Information | Yes |
|[ReservedCmdletChar](./ReservedCmdletChar.md) | Error | |
|[ReservedParams](./ReservedParams.md) | Error | |
|[ReviewUnusedParameter](./ReviewUnusedParameter.md) | Warning | |
|[ShouldProcess](./ShouldProcess.md) | Error | |
|[UseApprovedVerbs](./UseApprovedVerbs.md) | Warning | |
|[UseBOMForUnicodeEncodedFile](./UseBOMForUnicodeEncodedFile.md) | Warning | |
|[UseCmdletCorrectly](./UseCmdletCorrectly.md) | Warning | |
|[UseCorrectCasing](./UseCorrectCasing.md) | Information | |
|[UseDeclaredVarsMoreThanAssignments](./UseDeclaredVarsMoreThanAssignments.md) | Warning | |
|[UseLiteralInitializerForHashtable](./UseLiteralInitializerForHashtable.md) | Warning | |
|[UseOutputTypeCorrectly](./UseOutputTypeCorrectly.md) | Information | |
|[UseProcessBlockForPipelineCommand](./UseProcessBlockForPipelineCommand.md) | Warning | |
|[UsePSCredentialType](./UsePSCredentialType.md) | Warning | |
|[UseShouldProcessForStateChangingFunctions](./UseShouldProcessForStateChangingFunctions.md) | Warning | |
|[UseSingularNouns<sup>*</sup>](./UseSingularNouns.md) | Warning | |
|[UseSupportsShouldProcess](./UseSupportsShouldProcess.md) | Warning | |
|[UseToExportFieldsInManifest](./UseToExportFieldsInManifest.md) | Warning | |
|[UseCompatibleCmdlets](./UseCompatibleCmdlets.md) | Warning | Yes |
|[UseCompatibleCommands](./UseCompatibleCommands.md) | Warning | Yes |
|[UseCompatibleSyntax](./UseCompatibleSyntax.md) | Warning | Yes |
|[UseCompatibleTypes](./UseCompatibleTypes.md) | Warning | Yes |
|[PlaceOpenBrace](./PlaceOpenBrace.md) | Warning | Yes |
|[PlaceCloseBrace](./PlaceCloseBrace.md) | Warning | Yes |
|[UseConsistentIndentation](./UseConsistentIndentation.md) | Warning | Yes |
|[UseConsistentWhitespace](./UseConsistentWhitespace.md) | Warning | Yes |
|[UseUTF8EncodingForHelpFile](./UseUTF8EncodingForHelpFile.md) | Warning | |

<sup>*</sup> Rule is not available on all PowerShell versions, editions and/or OS platforms. See the rule's documentation for details.
