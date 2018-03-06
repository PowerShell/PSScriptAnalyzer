# Creating a Release

- Update changelog (`changelog.md`) with the new version number and change set. When updating the changelog please follow the same pattern as that of previous change sets (otherwise this may break the next step).
- Import the ReleaseMaker module and execute `New-Release` cmdlet to perform the following actions.
  - Update module manifest (engine/PSScriptAnalyzer.psd1) with the new version number and change set
  - Update the version number in `Engine/Engine.csproj` and `Rules/Rules.csproj`
  - Create a release build in `out/`
  - Build on a platform which supports both net451 and netstandard1.6 (Windows).

```powershell
    PS> Import-Module .\Utils\ReleaseMaker.psm1
    PS> New-Release
```

- Sign the binaries and PowerShell files in the release build and publish the module to [PowerShell Gallery](www.powershellgallery.com).
- Create a PR on `development` branch, with all the changes made in the previous step.
- Merge the changes to `development` and then merge `development` to `master` (Note that the `development` to `master` merge should be a `fast-forward` merge).
- Draft a new release on github and tag `master` with the new version number.
