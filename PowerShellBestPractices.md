#PowerShell Best Practices

The following guidelines come from a combined effort from both the PowerShell team and the community. We collect them from three different sources: Cmdlet Development Guidelines from MSDN site (Cmdlet Development Guidelines), The Community Book of PowerShell Practices and feedbacks from PowerSehll team members. We will use this guideline to define rules for PSScriptAnalyzer. Please feel free to propose additional guidelines and rules for PSScriptAnalyzer.

##From Cmdlet Development Guidelines
  1. Required Development Guidelines
    -Use Only Approved Verbs
    -Cmdlets Names: Characters that cannot be Used
    - Parameter Names that cannot be Used
    - Support Confirmation Requests 
        For cmdlets that perform an operation that modifies the system, they should call the ShouldProcess method to request confirmation, and in special cases call the ShouldContinue method     
    - Support Force Parameter for Interactive Sessions
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
