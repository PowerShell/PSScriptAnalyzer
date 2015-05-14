#ReturnCorrectTypeDSCFunctions 
**Severity Level: Information**


##Description

Set function in DSC class and Set-TargetResource in DSC resource must not return anything. Get function in DSC class must return an instance of the DSC class and Get-TargetResource function in DSC resource must return a hashtable. Test function in DSC class and Get-TargetResource function in DSC resource must return a boolean.


##How to Fix

To fix a violation of this rule, please correct the return type of the Get/Set/Test-TargetResource functions in DSC resource.


##Example


