# UseConsistentParametersKind

**Severity Level: Warning**

## Description

All functions should have same parameters definition kind specified in the rule.
Possible kinds are:
1. `Inline`, i.e.:
```PowerShell
function f([Parameter()]$FirstParam) {
    return
}
```
2. `ParamBlock`, i.e.:
```PowerShell
function f {
    param([Parameter()]$FirstParam)
    return
}
```

* For information: in simple scenarios both function definitions above may be considered as equal. Using this rule as-is is more for consistent code-style than functional, but it can be useful in combination with other rules.

## How to Fix

Rewrite function so it defines parameters as specified in the rule

## Example

### When the rule sets parameters definition kind to 'Inline':
```PowerShell
# Correct
function f([Parameter()]$FirstParam) {
    return
}

# Incorrect
function g {
    param([Parameter()]$FirstParam)
    return
}
```

### When the rule sets parameters definition kind to 'ParamBlock':
```PowerShell
# Inorrect
function f([Parameter()]$FirstParam) {
    return
}

# Correct
function g {
    param([Parameter()]$FirstParam)
    return
}
```