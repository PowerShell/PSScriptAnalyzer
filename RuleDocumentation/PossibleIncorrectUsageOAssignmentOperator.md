# PossibleIncorrectUsageOfAssignmentOperator

**Severity Level: Information**

## Description

In many programming languages, the equality operator is denoted as `==` or `=` in many programming languages, but `PowerShell` uses `-eq`. Therefore it can easily happen that the wrong operator is used unintentionally and this rule catches a few special cases where the likelihood of that is quite high.

The rule looks for usages of `==` and `=` operators inside `if`, `else if`, `while` and `do-while` statements but it will not warn if any kind of command or expression is used at the right hand side as this is probably by design.

## Example

### Wrong

```` PowerShell
if ($a = $b)
{
    ...
}
````

```` PowerShell
if ($a == $b)
{

}
````

### Correct

```` PowerShell
if ($a -eq $b) # Compare $a with $b
{
    ...
}
````

```` PowerShell
if ($a = Get-Something) # Only execute action if command returns something and assign result to variable
{
    Do-SomethingWith $a
}
````

## Implicit suppresion using Clang style

There are some rare cases where assignment of variable inside an if statement is by design. Instead of suppression the rule, one can also signal that assignment was intentional by wrapping the expression in extra parenthesis. An exception for this is when `$null` is used on the LHS is used because there is no use case for this.

```` powershell
if (($shortVariableName = $SuperLongVariableName['SpecialItem']['AnotherItem']))
{
    ...
}
````