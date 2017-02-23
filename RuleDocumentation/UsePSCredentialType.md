# UsePSCredentialType

**Severity Level: Warning**

## Description

If the cmdlet or function has a `Credential` parameter, the parameter must accept the `PSCredential` type.

## How

Change the `Credential` parameter's type to be `PSCredential`.

## Example

### Wrong

``` PowerShell
function Credential([String]$Credential)
{
	...
}
```

### Correct

``` PowerShell
function Credential([PSCredential]$Credential)
{
	...
}
```
