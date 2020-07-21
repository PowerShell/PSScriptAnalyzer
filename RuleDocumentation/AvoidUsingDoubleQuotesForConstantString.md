# AvoidUsingDoubleQuotesForConstantString

**Severity Level: Information**

## Description

When the value of a string is constant (i.e. not being interpolated by injecting variables or expression into such as e.g. `"$PID-$(hostname)"`), then single quotes should be used to express the constant nature of the string. This is not only to make the intent clearer that the string is a constant and makes it easier to use some special characters such as e.g. `$` within that string expression without the need to escape them. There are exceptions to that when double quoted strings are more readable though, e.g. when the string value itself has to contain a single quote (which would require a double single quotes to escape the character itself) or certain very special characters such as e.g. `"\n"` are already being escaped, the rule would not warn in this case as it is up to the author to decide on what is more readable in those cases.

## Example

### Wrong

``` PowerShell
$constantValue = "I Love PowerShell"
```

### Correct

``` PowerShell
$constantValue = 'I Love PowerShell'
```
