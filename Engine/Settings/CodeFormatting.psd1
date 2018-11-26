@{
    IncludeRules = @(
        'PSPlaceOpenBrace',
        'PSPlaceCloseBrace',
        'PSUseConsistentWhitespace',
        'PSUseConsistentIndentation',
        'PSAlignAssignmentStatement'
    )

    Rules        = @{
        PSPlaceOpenBrace           = @{
            Enable             = $true
            OnSameLine         = $true
            NewLineAfter       = $true
            IgnoreOneLineBlock = $true
        }

        PSPlaceCloseBrace          = @{
            Enable             = $true
            NewLineAfter       = $true
            IgnoreOneLineBlock = $true
            NoEmptyLineBefore  = $false
        }

        PSUseConsistentIndentation = @{
            Enable          = $true
            Kind            = 'space'
            PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
            IndentationSize = 4
        }

        PSUseConsistentWhitespace  = @{
            Enable         = $true
            CheckOpenBrace = $true
            CheckOpenParen = $true
            CheckOperator  = $true
            CheckSeparator = $true
        }

        PSAlignAssignmentStatement = @{
            Enable         = $true
            CheckHashtable = $true
        }
    }
}
