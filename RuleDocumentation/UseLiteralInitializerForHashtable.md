# UseLiteralInitializerForHashtable

**Severity Level: Warning**

## Description

Creating a hashtable by either `[hashtable]::new()` or `New-Object -TypeName hashtable` will create a hashtable wherein the keys are looked-up in a case-sensitive manner, unless an `IEqualityComparer` object is passed as an argument. However, PowerShell is case-insensitive in nature and it is best to create hashtables with case-insensitive key look-up. This rule is intended to warn the author of the case-sensitive nature of the hashtable if he/she creates a hashtable using the `new` member or the `New-Object` cmdlet.

## How to Fix

Use the full cmdlet name and not an alias.

## Example

### Wrong???

``` PowerShell
$hashtable = [hashtable]::new()
```

### Wrong???

``` PowerShell
$hashtable = New-Object -TypeName hashtable
```

### Correct:

``` PowerShell
$hashtable = @{}
```
