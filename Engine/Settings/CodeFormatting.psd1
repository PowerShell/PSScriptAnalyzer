@{
    IncludeRules = @(
        'PSPlaceOpenBrace',
        'PSPlaceCloseBrace',
        'PSUseConsistentIndentation'
    )

    Rules = @{
        PSPlaceOpenBrace = @{
            Enable = $true
            OnSameLine = $true
            NewLineAfter = $true
        }

        PSPlaceCloseBrace = @{
            Enable = $true
        }

        PSUseConsistentIndentation = @{
            Enable = $true
            IndentationSize = 4
        }
    }
}
