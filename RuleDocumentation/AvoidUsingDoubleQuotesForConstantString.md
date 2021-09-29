# AvoidUsingDoubleQuotesForConstantString

**Severity Level: Information**

## Description

Single quotes should be used when the value of a string is constant. A constant string doesn't
contain variables or expressions intended to insert values into the string, such as
`"$PID-$(hostname)"`).

This makes the intent clearer that the string is a constant and makes it easier to use some special
characters such as `$` within that string expression without needing to escape them.

There are exceptions to that when double quoted strings are more readable. For example, when the
string value itself must contain a single quote or other special characters, such as newline
(`` "`n" ``), are already being escaped. The rule does not warn in these cases.

## Example

### Wrong

```powershell
$constantValue = "I Love PowerShell"
```

### Correct

```powershell
$constantValue = 'I Love PowerShell'
```
