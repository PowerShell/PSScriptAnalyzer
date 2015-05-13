#UseDeclaredVarsMoreThanAssignments 
**Severity Level: Warning**


##Description

Checks that variables are used in more than just their assignment. Generally this is a red flag that a variable is not needed. This rule does not check if the assignment and usage are in the same function.


##How to Fix

Please consider remove the variables that are declared but not used outside of the function.


##Example
Wrong： 
    
    function Test
    {
        $declaredVar = "Declared"
        $declaredVar2 = "Not used"
    }

Correct: 

    function Test
    {
        $declaredVar = "Declared just for fun"
        Write-Output $declaredVar
    }

