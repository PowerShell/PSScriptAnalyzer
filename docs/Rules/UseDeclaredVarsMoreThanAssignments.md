# UseDeclaredVarsMoreThanAssignments

**Severity Level: Warning**

## Description

Generally variables that are not used more than their assignments are considered wasteful and not
needed.

## How

Remove the variables that are declared but not used.

## Example

### Wrong

```powershell
function Test
{
    $declaredVar = "Declared just for fun"
    $declaredVar2 = "Not used"
    Write-Output $declaredVar
}
```

### Correct

```powershell
function Test
{
    $declaredVar = "Declared just for fun"
    Write-Output $declaredVar
}
```
