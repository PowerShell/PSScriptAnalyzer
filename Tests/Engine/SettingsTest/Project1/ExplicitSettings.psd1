@{
    "IncludeRules" = @("PSAvoidUsingCmdletAliases", "PSAvoidUsingWriteHost")
    "ExcludeRules" = @("PSShouldProcess", "PSAvoidUsingWMICmdlet", "PSUseCmdletCorrectly")
    "rules" = @{
        PSAvoidUsingCmdletAliases = @{
            WhiteList = @("cd", "cp")
        }
    }
}