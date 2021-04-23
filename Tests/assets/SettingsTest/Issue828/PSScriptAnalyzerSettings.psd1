@{
    Severity     = @(
        'Error',
        'Warning',
        'Information'
    )
    ExcludeRules = @(
        'PSUseOutputTypeCorrectly',
        'PSUseShouldProcessForStateChangingFunctions'
    )
    Rules        = @{
        PSAlignAssignmentStatement = @{
            Enable         = $true
            CheckHashtable = $true
        }
        PSAvoidUsingCmdletAliases  = @{
            # only allowlist verbs from *-Object cmdlets
            allowlist = @(
                '%',
                '?',
                'compare',
                'foreach',
                'group',
                'measure',
                'select',
                'sort',
                'tee',
                'where'
            )
        }
        PSPlaceCloseBrace          = @{
            Enable             = $true
            NoEmptyLineBefore  = $false
            IgnoreOneLineBlock = $true
            NewLineAfter       = $false
        }
        PSPlaceOpenBrace           = @{
            Enable             = $true
            OnSameLine         = $true
            NewLineAfter       = $true
            IgnoreOneLineBlock = $true
        }
        PSProvideCommentHelp       = @{
            Enable                  = $true
            ExportedOnly            = $true
            BlockComment            = $true
            VSCodeSnippetCorrection = $true
            Placement               = "before"
        }
        PSUseConsistentIndentation = @{
            Enable          = $true
            IndentationSize = 4
            Kind            = "space"
        }
        PSUseConsistentWhitespace  = @{
            Enable         = $true
            CheckOpenBrace = $true
            CheckOpenParen = $true
            CheckOperator  = $false
            CheckSeparator = $true
        }
    }
}
