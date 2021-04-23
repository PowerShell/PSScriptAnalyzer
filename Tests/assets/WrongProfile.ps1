@{
    Severity='Warning'
    IncludeRules=@('PSAvoidUsingCmdletAliases',
                    'PSAvoidUsingPositionalParameters',
                    'PSAvoidUsingInternalURLs')
    ExcludeRules=@(1)
    Exclude=@('blah')
}