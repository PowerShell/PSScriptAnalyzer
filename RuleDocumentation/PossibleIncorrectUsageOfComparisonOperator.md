# PossibleIncorrectUsageOfComparisonOperator

** Severity Level: Warning **

## Description

In many programming languages, the comparison operator for greater is `>` but `PowerShell` uses `-gt` for it and `-ge` for `>=`. Similarly the equality operator is denoted as `==` or `=` in many programming languages, but `PowerShell` uses `-eq`. Since using as the FileRedirection operator `>` or the assignment operator are rarely needed inside if statements, this rule wants to call out this case because it was probably unintentional.

The rule looks for usages of `==`, `=` and `>` operators inside if statements and for the case of assignments, it will only warn if the variable is not being used at all in the statement block to avoid false positives because assigning a variable inside an if statement is an elegant way of getting an object and performing an implicit null check on it in one line.

## Example

### Wrong

``` PowerShell
if ($superman > $batman)
{

}
```

``` PowerShell
if ($superman == $batman)
{

}
```

``` PowerShell
if ($superman = $batman)
{
    # Not using the assigned variable is an indicator that assignment was either by accident or unintentional
}
```

### Correct

``` PowerShell
if ($superman = Get-Superman)
{
    Save-World $superman
}
```
