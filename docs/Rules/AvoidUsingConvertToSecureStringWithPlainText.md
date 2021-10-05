# AvoidUsingConvertToSecureStringWithPlainText

**Severity Level: Error**

## Description

The use of the `AsPlainText` parameter with the `ConvertTo-SecureString` command can expose secure
information.

## How

Use a standard encrypted variable to perform any SecureString conversions.

## Recommendations

If you do need an ability to retrieve the password from somewhere without prompting the user,
consider using the
[SecretStore](https://www.powershellgallery.com/packages/Microsoft.PowerShell.SecretStore)
module from the PowerShell Gallery.

## Example

### Wrong

```powershell
$UserInput = Read-Host "Please enter your secure code"
$EncryptedInput = ConvertTo-SecureString -String $UserInput -AsPlainText -Force
```

### Correct

```powershell
$SecureUserInput = Read-Host "Please enter your secure code" -AsSecureString
$EncryptedInput = ConvertFrom-SecureString -String $SecureUserInput
$SecureString = ConvertTo-SecureString -String $EncryptedInput
```
