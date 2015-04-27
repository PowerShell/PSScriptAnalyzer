#PowerShell Best Practices

The following guidelines come from a combined effort from both the PowerShell team and the community. We collect them from three different sources: Cmdlet Development Guidelines from MSDN site (Cmdlet Development Guidelines), The Community Book of PowerShell Practices and feedbacks from PowerSehll team members. We will use this guideline to define rules for PSScriptAnalyzer. Please feel free to propose additional guidelines and rules for PSScriptAnalyzer.

##From Cmdlet Development Guidelines
  1. Required Development Guidelines
    -Use Only Approved Verbs
    -Cmdlets Names: Characters that cannot be Used
    - Parameter Names that cannot be Used
    - Support Confirmation Requests
            For cmdlets that perform an operation that modifies the system, they should call the ShouldProcess method to request confirmation, and in special cases call the ShouldContinue method     
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
    - Provide Feedback to the User 
    - Create a Cmdlet Help File 
    - Coding Parameters 
    - Support Well Defined Pipeline Input 
    - Write Single Records to the Pipeline 
    - Make Cmdlets Case-Insensitive and Case-Preserving 


##The Community Book of PowerShell Practices(Compiled by Don Jones and Matt Penny and the Windows PowerShell Community)

1. Write Help and Comments
    - Write comment-based help 
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
    - Use write-verbose to give information to someone running your script
    - Use write-debug to give information to someone maintaining your script
    - Use [CmdletBinding()] if you are using write-debug or write-verbose

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
    - Document why you haven't sued PowerShell
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
