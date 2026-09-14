@{
    Severity = @('Error', 'Warning', 'Information')
    IncludeRules = @(
        'PSAvoidUsingCmdletAliases', 'PSAvoidDefaultValueForMandatoryParameter',
        'PSAvoidDefaultValueSwitchParameter', 'PSAvoidGlobalAliases',
        'PSAvoidGlobalFunctions', 'PSAvoidGlobalVars', 'PSAvoidInvokingEmptyMembers',
        'PSAvoidNullOrEmptyHelpMessageAttribute', 'PSAvoidShouldContinueWithoutForce',
        'PSAvoidUsingComputerNameHardcoded', 'PSAvoidUsingConvertToSecureStringWithPlainText',
        'PSAvoidUsingDeprecatedManifestFields', 'PSAvoidUsingEmptyCatchBlock',
        'PSAvoidUsingInvokeExpression', 'PSAvoidUsingPlainTextForPassword',
        'PSAvoidUsingPositionalParameters', 'PSAvoidUsingUsernameAndPasswordParams',
        'PSAvoidUsingWMICmdlet', 'PSAvoidUsingWriteHost', 'PSMisleadingBacktick',
        'PSMissingModuleManifestField', 'PSPossibleIncorrectComparisonWithNull',
        'PSPossibleIncorrectUsageOfAssignmentOperator', 'PSPossibleIncorrectUsageOfRedirectionOperator',
        'PSProvideCommentHelp', 'PSReservedCmdletChar', 'PSReservedParams',
        'PSUseApprovedVerbs', 'PSUseBOMForUnicodeEncodedFile', 'PSUseCmdletCorrectly',
        'PSUseConsistentIndentation', 'PSUseConsistentWhitespace', 'PSUseCorrectCasing',
        'PSUseDeclaredVarsMoreThanAssignments', 'PSUseLiteralInitializerForHashtable',
        'PSUseOutputTypeCorrectly', 'PSUsePSCredentialType', 'PSUseSingularNouns',
        'PSUseToExportFieldsInManifest', 'PSUseUTF8EncodingForHelpFile'
    )
    ExcludeRules = @(
        'PSAvoidUsingWriteHost', 'PSAvoidUsingPositionalParameters', 'PSUseApprovedVerbs',
        'PSProvideCommentHelp', 'PSAvoidGlobalVars', 'PSAvoidGlobalFunctions',
        'PSUseSingularNouns', 'PSUseOutputTypeCorrectly'
    )
    Rules = @{
        PSUseConsistentIndentation = @{
            Enable = $true
            IndentationSize = 4
            PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
            Kind = 'space'
        }
        PSUseConsistentWhitespace = @{
            Enable = $true
            CheckInnerBrace = $true
            CheckOpenBrace = $true
            CheckOpenParen = $true
            CheckOperator = $true
            CheckPipe = $true
            CheckPipeForRedundantWhitespace = $false
            CheckSeparator = $true
            CheckParameter = $false
            IgnoreAssignmentOperatorInsideHashTable = $true
        }
        PSUseCompatibleCmdlets = @{ Enable = $false }
        PSUseCorrectCasing = @{ Enable = $true }
        PSAvoidUsingCmdletAliases = @{ Enable = $true; allowlist = @() }
        PSAlignAssignmentStatement = @{ Enable = $false; CheckHashtable = $false }
        PSPlaceOpenBrace = @{ Enable = $true; OnSameLine = $true; NewLineAfter = $true; IgnoreOneLineBlock = $true }
        PSPlaceCloseBrace = @{ Enable = $true; NewLineAfter = $true; IgnoreOneLineBlock = $true; NoEmptyLineBefore = $false }
    }
}
