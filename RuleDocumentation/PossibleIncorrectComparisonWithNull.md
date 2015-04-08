#PossibleIncorrectComparisonWithNull 
**Severity Level: Warning**


##Description

Checks that $null is on the left side of any equaltiy comparisons (eq, ne, ceq, cne, ieq, ine). When there is an array on the left side of a null equality comparison, PowerShell will check for a $null IN the array rather than if the array is null. If the two sides of the comaprision are switched this is fixed. Therefore, $null should always be on the left side of equality comparisons just in case.

##How to Fix

Please consider moving null on the left side of the comparison.
##Example

Wrong： 

	function CompareWithNull
	{
	    if ($DebugPreference -eq $null) 
	    {
	    }
	}

Correct: 

	function CompareWithNull
	{
	    if ($null -eq $DebugPreference) 
	    {
	    }
	}
