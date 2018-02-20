# PossibleIncorrectUsageOfComparisonOperator

**Severity Level: Information**

## Description

In many programming languages, the comparison operator for 'greater than' is `>` but `PowerShell` uses `-gt` for it and `-ge` (greater or equal) for `>=`. Similarly the equality operator is denoted as `==` or `=` in many programming languages, but `PowerShell` uses `-eq`. Therefore it can easily happen that the wrong operator is used unintentionally and this rule catches a few special cases where the likelihood of that is quite high.

The rule looks for usages of `==`, `=` and `>` operators inside if statements and for the case of assignments, it will only warn if the variable is not being used at all in the statement block to avoid false positives because assigning a variable inside an if statement is an elegant way of getting an object and performing an implicit null check on it in one line.

## Example

### Wrong

```` PowerShell
if ($superman > $batman)
{

}
````

```` PowerShell
if ($superman == $batman)
{

}
````

```` PowerShell
if ($superman = $batman)
{
    # Not using the assigned variable is an indicator that assignment was either by accident or unintentional
}
````

### Correct

```` PowerShell
if ($superman = Get-Superman)
{
    Save-World $superman
}
````