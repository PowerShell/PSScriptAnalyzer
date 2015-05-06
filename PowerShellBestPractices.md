#PowerShell Best Practices

The following guidelines come from a combined effort from both the PowerShell team and the community. We collect them from three different sources: Cmdlet Development Guidelines from MSDN site (Cmdlet Development Guidelines), The Community Book of PowerShell Practices and feedbacks from PowerSehll team members. We will use this guideline to define rules for PSScriptAnalyzer. Please feel free to propose additional guidelines and rules for PSScriptAnalyzer.
**Note: The links next to each guidelines are rules we already implmented as built-in rules.

##From Cmdlet Development Guidelines
  1. Required Development Guidelines
    - Use Only Approved Verbs [UseApprovedVerbs](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseApprovedVerbs.md)
    - Cmdlets Names: Characters that cannot be Used [AvoidReservedCharInCmdlet](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidReservedCharInCmdlet.md)
    - Parameter Names that cannot be Used [AvoidReservedParams](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidReservedParams.md)
    - Support Confirmation Requests [UseShouldProcessCorrectly](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseShouldProcessCorrectly.md) and [UseShouldProcessForStateChangingFunctions](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseShouldProcessForStateChangingFunctions.md)
      - For cmdlets that perform an operation that modifies the system, they should call the ShouldProcess method to request confirmation, and in special cases call the ShouldContinue method     
    - Support Force Parameter for Interactive Session
            If your cmdlet is used interactively, always provide a Force parameter to override the interactive actions, such as prompts or reading lines of input). This is important because it allows your cmdlet to be used in non-interactive scripts and hosts. The following methods can be implemented by an interactive host.
    - Document Output Objects

  2. Core Guidelines
    - Derive from the Cmdlet or PSCmdlet Classes
    - Specify the Cmdlet Attribute
    - Override an Input Processing Method
    - Specify the OutputType Attribute
    - Do Not Retain Handles on Output Objects
    - Handle Errors Robustly
    - Use a Windows PowerShell Module to Deploy your Cmdlets

  3. Strongly Encouraged Development Guidelines
    - Use a Specific Noun for a Cmdlet Name 
    - Use Pascal Case for Cmdlet Names
    - Parameter Design Guidelines 
    - Provide Feedback to the User [ProvideVerboseMessage](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/ProvideVerboseMessage.md)
    - Create a Cmdlet Help File 
    - Coding Parameters 
    - Support Well Defined Pipeline Input 
    - Write Single Records to the Pipeline 
    - Make Cmdlets Case-Insensitive and Case-Preserving 


##The Community Book of PowerShell Practices

(Compiled by Don Jones and Matt Penny and the Windows PowerShell Community)

1. Write Help and Comments
    - Write comment-based help [ProvideCommentHelp](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/ProvideCommentHelp.md)
    - Describe each parameter 
    - Provide usage Examples
    - Use the Notes section for detail on how the tool works
    - Keep your language simple
    - Comment your code
    - Don't over-comment

2. Versioning
    - Write for the lowest version of PowerShell that you can
    - Document the version of PowerShell that script was written for
    

3. Performance
    - If performance matters, test it
    - Consider trade-offs between performance and readability

4. Key Practices: Aesthetics
    - Indent your code
    - Avoid backticks

5. Output
    - Don't use write-host unless writing to the host is all you want to do 
    - Use write-verbose to give information to someone running your script [ProvideVerboseMessage](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/ProvideVerboseMessage.md)

6. Tools vs. Controller
    - Decide whether you are coding a tool or a controller script
    - Make your code modular
    - Make tools as re-usable as possible
    - Use PowerShell standard cmdlet naming
    - Use PowerShell standard parameter naming
    - Tools should output raw data
    - Controllers should typically output formatted data

7. The Purity Laws
    - Use native PowerShell where possible
    - If you can't use just PowerShell, use.net, external commands or COM objects, in that order of preference
    - Document why you haven't used PowerShell
    - Wrap other tools in an advance function of cmdlet

8. Pipelines vs. Constructs
    - Avoid using pipelines in scripts

9. Trapping and Capturing Errors
    - Use -ErrorAction Stop when calling cmdlets
    - Use $ErrorActionPreference = 'Stop'/' Continue' when calling non-cmdlets
    - Avoid using flags to handle errors
    - Avoid using $?
    - Avoid testing for a null variable as an error condition
    - Copy $Error[0] to your own variable


##By PowerShell team members

1. Required Rules for publishing
    - Module must be loadable
    - No syntax errors
    - Module Manifest Fields [MissingModuleManifestField](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/MissingModuleManifestField.md)
      - Version
      - Author
      - Description
      - LicenseUri (for PowerShell Gallery)
    - Unresolved dependencies are an error
    - Must call ShouldProcess when ShouldProcess attribute is present and vice versa.
    - Switch parameters should not default to true 
     
 
2. Script
    - Non-global variables must be initialized. Those that are supposed to be global and not initialized must have “global:” (includes for loop initializations) [AvoidGlobalVars](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidGlobalVars.md)
    - Declared variables must be used in more than just their assignment. [UseDeclaredVarsMoreThanAssignments](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseDeclaredVarsMoreThanAssignments.md)
    - No traps [AvoidTrapStatement](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidTrapStatement.md)
    - No Invoke-Expression [AvoidUsingInvokeExpression](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingInvokeExpression.md)
    - If a return type is declared, the cmdlet must return that type. If a type is returned, a return type must be declared.
    - Cmdlets should have ShouldProcess/ShouldContinue  and Force param if certain system-modding verbs are present (Update, Set, Remove, New)
    - Should never have both -Username and -Password parameters (should take credentials)[UsePSCredentialType](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UsePSCredentialType.md)
    - Should not use Write-Host or Console.Writeline
    - -Credential should take PSCredential as an argument, -Force is switch 
    - Nouns should be singular [UseSingularNouns](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseSingularNouns.md)
    - Should have help on every exported command (including parameter documentation
    - Password = 'string' should not be used. (information disclosure) [AvoidUsingUsernameAndPasswordParams](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingUsernameAndPasswordParams.md)
    - ConvertTo-SecureString with plaintext should be warning (information disclosure) 
    - APIKey and Credentials variables that are initialized (information disclosure)
    - Internal URLs should not be used (information disclosure)[AvoidUsingFilePath](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingFilePath.md)
    - -ComputerName hardcoded should not be used (information disclosure)[AvoidUsingComputerNameHardcoded](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingComputerNameHardcoded.md)
    - -Password should be secure string [AvoidUsingPlainTextForPassword](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingPlainTextForPassword.md)
    - Clear-Host should not be used
    - File paths should not be used (UNC)
    - Emtpy catch block should not be used [AvoidEmptyCatchBlock](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidEmptyCatchBlock.md)
    - Avoid using alias [AvoidAlias](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidAlias.md)
    - Avoid using deprecated WMI cmdlets [AvoidUsingWMICmdlet](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/AvoidUsingWMICmdlet.md)

3. DSC
    - For PowerShell V4: Resource module contains .psd1 file and schema.mof for every resource 
    - For PowerShell V5: MOF has description for each element
    - PowerShell V5: support class-based resources. If class-based resource, no MOF needed. (need to support these at same time as schema.mof test exists) 
    - Resource module contains Resources folder which contains the resources
    - Use standard DSC methods [UseStandardDSCFunctionsInResource](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseStandardDSCFunctionsInResource.md)
    - Use identical mandatory parameters for all DSC methods [UseIdenticalMandatoryParametersDSC](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseIdenticalMandatoryParametersDSC.md)
    - Use identical parameters for Set and Test DSC methods [UseIdenticalParametersDSC](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/UseIdenticalParametersDSC.md)
    - Use ShouldProcess for a Set DSC method
    - All of the following three rule are grouped by: [ReturnCorrectTypeDSCFunctions](https://github.com/PowerShell/PSScriptAnalyzer/blob/master/RuleDocumentation/ReturnCorrectTypeDSCFunctions.md)
      - Avoid return any object from a Set-TargetResource function
      - Returning a Boolean object from a Test-TargetResource function
      - Returning an object from a Get-TargetResource function

