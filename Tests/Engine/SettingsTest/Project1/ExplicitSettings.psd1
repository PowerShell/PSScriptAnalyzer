@{
    "IncludeRules" = @("PSAvoidUsingCmdletAliases", "PSAvoidUsingWriteHost", "PSUseConsistentIndentation")
    "ExcludeRules" = @("PSShouldProcess", "PSAvoidUsingWMICmdlet", "PSUseCmdletCorrectly")
    "rules"        = @{
        PSAvoidUsingCmdletAliases  = @{
            WhiteList = @("cd", "cp")
        }

        PSUseConsistentIndentation = @{
            Enable = $true
            IndentationSize = 4
        }

        PSProvideCommentHelp       = @{
            Enable = $true
            Placement = 'end'
        }
    }
}
