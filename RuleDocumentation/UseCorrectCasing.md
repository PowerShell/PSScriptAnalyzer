# UseCorrectCasing

**Severity Level: Information**

## Description

This is a style/formatting rule. PowerShell is case insensitive where applicable. The casing of cmdlet names does not matter but this rule ensures that the casing matches for consistency and also because most cmdlets start with an upper case and using that improves readability to the human eye.

## How

Use exact casing of the cmdlet, e.g. `Invoke-Command`.

## Example

### Wrong

``` PowerShell
invoke-command { 'foo' }
}
```

### Correct

``` PowerShell
Invoke-Command { 'foo' }
}
```
