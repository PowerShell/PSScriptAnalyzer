# AlignAssignmentStatement

**Severity Level: Warning**

## Description

Consecutive assignment statements are more readable if they are aligned. By aligned, we imply that the `equal` sign for all the assignment statements should be in the same column.

The rule looks for key (property) value pairs in a hashtable (DSC configuration) to check if they are aligned or not. Consider the following example in which the key value pairs are not aligned.

```powershell
$hashtable = @{
    property1 = "value"
    anotherProperty = "another value"
}
```

Alignment in this case would look like the following.

```powershell
$hashtable = @{
    property1       = "value"
    anotherProperty = "another value"
}
```

The rule will ignore hashtables in which the assignment statements are on the same line. For example, the rule will ignore `$h = {a = 1; b = 2}`.

## Configuration

```powershell
    Rules = @{
        PSAlignAssignmentStatement = @{
            Enable = $true
            CheckHashtable = $true
        }
    }
```

### Parameters

#### Enable: bool (Default value is `$false`)

Enable or disable the rule during ScriptAnalyzer invocation.

#### CheckHashtable: bool (Default value is `$false`)

Enforce alignment of assignment statements in a hashtable and in a DSC Configuration. There is only one switch for hasthable and DSC configuration because the property value pairs in a DSC configuration are parsed as key value pairs of a hashtable.
