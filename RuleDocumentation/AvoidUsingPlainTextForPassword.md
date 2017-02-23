# AvoidUsingPlainTextForPassword

**Severity Level: Warning**

## Description

Password parameters that take in plaintext will expose passwords and compromise the security of your system. Passwords should be stored in the
```SecureString``` type.

The following parameters are considered password parameters (this is not case sensitive):
* Password
* Pass
* Passwords
* Passphrase
* Passphrases
* PasswordParam

If a parameter is defined with a name in the above list, it should be declared with type ```SecureString```

## How

Change the type to ```SecureString```.

## Example

### Wrong

``` PowerShell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [string]
        $Password
    )
    ...
}
```

### Correct

``` PowerShell
function Test-Script
{
    [CmdletBinding()]
    Param
    (
        [SecureString]
        $Password
    )
    ...
}
```
