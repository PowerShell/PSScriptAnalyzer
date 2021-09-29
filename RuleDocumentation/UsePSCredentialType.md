# UsePSCredentialType

**Severity Level: Warning**

## Description

If the cmdlet or function has a **Credential** parameter, the parameter must accept the
**PSCredential** type.

## How

Change the **Credential** parameter's type to be **PSCredential**.

## Example

### Wrong

```powershell
function Credential([String]$Credential)
{
    ...
}
```

### Correct

```powershell
function Credential([PSCredential]$Credential)
{
    ...
}
```
