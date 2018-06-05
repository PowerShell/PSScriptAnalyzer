# PowerShell Best Practices

The following guidelines come from a combined effort from both the PowerShell team and the community. We will use this guideline to define rules for `PSScriptAnalyzer`. Please feel free to propose additional guidelines and rules for `PSScriptAnalyzer`.
**Note**: The hyperlink next to each guidelines will redirect to documentation page for the rule that is already implemented.

## Cmdlet Design Rules

### Severity: Error

### Severity: Warning

- Use Only Approved Verbs [UseApprovedVerbs](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseApprovedVerbs.md)
- Cmdlets Names: Characters that cannot be Used [AvoidReservedCharInCmdlet](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/ReservedCmdletChar.md)
- Parameter Names that cannot be Used [AvoidReservedParams](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/ReservedParams.md)
- Support Confirmation Requests [UseShouldProcessForStateChangingFunctions](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseShouldProcessForStateChangingFunctions.md) and [UseShouldProcessForStateChangingFunctions](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseShouldProcessForStateChangingFunctions.md)
- Must call ShouldProcess when ShouldProcess attribute is present and vice versa.[UseShouldProcess](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/ShouldProcess.md)
- Nouns should be singular [UseSingularNouns](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseSingularNouns.md)
- Module Manifest Fields [MissingModuleManifestField](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/MissingModuleManifestField.md)
  - Version
  - Author
  - Description
  - LicenseUri (for PowerShell Gallery)
- Switch parameters should not default to true  [AvoidDefaultValueSwitchParameter](https://github.com/PowetrShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidDefaultValueSwitchParameter.md)

### Severity: Information

### Severity: TBD

- Support Force Parameter for Interactive Session
- If your cmdlet is used interactively, always provide a Force parameter to override the interactive actions, such as prompts or reading lines of input). This is important because it allows your cmdlet to be used in non-interactive scripts and hosts. The following methods can be implemented by an interactive host.
- Document Output Objects
- Module must be loadable
- No syntax errors
- Unresolved dependencies are an error
- Derive from the Cmdlet or PSCmdlet Classes
- Specify the Cmdlet Attribute
- Override an Input Processing Method
- Specify the OutputType Attribute
- Write Single Records to the Pipeline 
- Make Cmdlets Case-Insensitive and Case-Preserving 

## Script Functions

### Severity: Error

### Severity: Warning

- Avoid using alias [AvoidUsingCmdletAliases](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingCmdletAliases.md)
- Avoid using deprecated WMI cmdlets [AvoidUsingWMICmdlet](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingWMICmdlet.md)
- Empty catch block should not be used [AvoidUsingEmptyCatchBlock](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingEmptyCatchBlock.md)
- Invoke existing cmdlet with correct parameters [UseCmdletCorrectly](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseCmdletCorrectly.md)
- Cmdlets should have ShouldProcess/ShouldContinue and Force param if certain system-modding verbs are present (Update, Set, Remove, New): [UseShouldProcessForStateChangingFunctions](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseShouldProcessForStateChangingFunctions.md)
- Positional parameters should be avoided [AvoidUsingPositionalParameters](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingPositionalParameters.md)
- Global variables should be avoided. [AvoidGlobalVars](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidGlobalVars.md)
- Declared variables must be used in more than just their assignment. [UseDeclaredVarsMoreThanAssignments](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseDeclaredVarsMoreThanAssignments.md)
- No Invoke-Expression [AvoidUsingInvokeExpression](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingInvokeExpression.md)

### Severity: Information

### Severity: TBD

- `Clear-Host` should not be used
- File paths should not be used (UNC)
- Error Handling
  - Use `-ErrorAction Stop` when calling cmdlets
  - Use $ErrorActionPreference = 'Stop'/' Continue' when calling non-cmdlets
  - Avoid using flags to handle errors
  - Avoid using `$?`
  - Avoid testing for a null variable as an error condition
  - Copy `$Error[0]` to your own variable
- Avoid using pipelines in scripts
- If a return type is declared, the cmdlet must return that type. If a type is returned, a return type must be declared.

## Scripting Style

### Severity: Error

### Severity: Warning 

- Don't use `Write-Host` unless writing to the host is all you want to do [AvoidUsingWriteHost](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingWriteHost.md)

### Severity: Information

- Write comment-based help [ProvideCommentHelp](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/ProvideCommentHelp.md)

### Severity: TBD

- Provide usage Examples
- Use the Notes section for detail on how the tool work
- Should have help on every exported command (including parameter documentation
- Document the version of PowerShell that script was written for  
- Indent your code
- Avoid backticks

## Script Security

### Severity: Error

- Password should be secure string [AvoidUsingPlainTextForPassword](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingPlainTextForPassword.md)- Should never have both -Username and -Password parameters (should take credentials): [UsePSCredentialType](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UsePSCredentialType.md)
- `-ComputerName` Parameter argument hardcoded should not be used (information disclosure): [AvoidUsingComputerNameHardcoded](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingComputerNameHardcoded.md)
- ConvertTo-SecureString with plaintext should not be used (information disclosure): [AvoidUsingConvertToSecureStringWithPlainText](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingConvertToSecureStringWithPlainText.md)

### Severity: Warning

- Password = 'string' should not be used. (information disclosure) [AvoidUsingUsernameAndPasswordParams](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingUsernameAndPasswordParams.md)

### Severity: Information

### Severity: TBD

- APIKey and Credentials variables that are initialized (information disclosure)

## DSC Related Rules

### Severity: Error

- Use standard DSC methods [StandardDSCFunctionsInResource](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/StandardDSCFunctionsInResource.md)
- Use identical mandatory parameters for all DSC methods [UseIdenticalMandatoryParametersForDSC](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseIdenticalMandatoryParametersForDSC.md)
- Use identical parameters for Set and Test DSC methods [UseIdenticalParametersForDSC](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseIdenticalParametersForDSC.md)

### Severity: Warning

### Severity: Information

- All of the following three rule are grouped by: [ReturnCorrectTypesForDSCFunctions](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/ReturnCorrectTypesForDSCFunctions.md)
- Avoid return any object from a `Set-TargetResource` or Set (Class Based) function
- Returning a Boolean object from a `Test-TargetResource` or Test (Class Based) function
- Returning an object from a `Get-TargetResource` or Get (Class Based) function
- DSC resources should have DSC tests [DSCTestsPresent](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/DscTestsPresent.md)
- DSC resources should have DSC examples [DSCExamplesPresent](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/DscExamplesPresent.md)

### Severity: TBD

- For PowerShell V4: Resource module contains `.psd1` file and `schema.mof` for every resource
- MOF has description for each element [IssueOpened](https://github.com/PowerShell/PSScriptAnalyzer/issues/131)
- Resource module must contain .psd1 file (always) and schema.mof (for non-class resource). [IssueOpened](https://github.com/PowerShell/PSScriptAnalyzer/issues/116)
- Use ShouldProcess for a Set DSC method
- Resource module contains DscResources folder which contains the resources [IssueOpened](https://github.com/PowerShell/PSScriptAnalyzer/issues/130)

### Reference

* Cmdlet Development Guidelines from MSDN site (Cmdlet Development Guidelines): https://msdn.microsoft.com/en-us/library/ms714657(v=vs.85).aspx 
* The Community Book of PowerShell Practices (Compiled by Don Jones and Matt Penny and the Windows PowerShell Community): https://powershell.org/community-book-of-powershell-practices/
* PowerShell DSC Resource Design and Testing Checklist: https://blogs.msdn.com/b/powershell/archive/2014/11/18/powershell-dsc-resource-design-and-testing-checklist.aspx
* DSC Guidelines can also be found in the DSC Resources Repository: https://github.com/PowerShell/DscResources
* The Unofficial PowerShell Best Practices and Style Guide: https://github.com/PoshCode/PowerShellPracticeAndStyle
