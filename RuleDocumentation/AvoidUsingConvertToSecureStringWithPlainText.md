# AvoidUsingConvertToSecureStringWithPlainText
**Severity Level: Error**

## Description
The use of the `AsPlainText` parameter with the `ConvertTo-SecureString` command can expose secure information.

## How
Use a standard encrypted variable to perform any SecureString conversions.

## Example
### Wrong
``` PowerShell
$UserInput = Read-Host "Please enter your secure code"
$EncryptedInput = ConvertTo-SecureString -String $UserInput -AsPlainText -Force
```

### Correct
``` PowerShell
$SecureUserInput = Read-Host "Please enter your secure code" -AsSecureString
$EncryptedInput = ConvertFrom-SecureString -String $SecureUserInput
$SecureString = Convertto-SecureString -String $EncryptedInput
```
