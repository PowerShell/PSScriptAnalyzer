@{
  IncludeRules = @(
      'PSUseCompatibleCmdlets'
  )
  Rules = @{
      PSUseCompatibleCmdlets = @{
          compatibility = @("core-6.1.0-preview.2-linux")
      }
  }
}
