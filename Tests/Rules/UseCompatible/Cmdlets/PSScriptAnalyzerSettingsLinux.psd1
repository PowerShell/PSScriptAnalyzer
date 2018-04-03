@{
  IncludeRules = @(
      'PSUseCompatibleCmdlets'
  )
  Rules = @{
      PSUseCompatibleCmdlets = @{
          compatibility = @("core-6.0.1-linux")
      }
  }
}