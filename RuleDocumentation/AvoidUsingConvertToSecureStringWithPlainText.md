# AvoidUsingConvertToSecureStringWithPlainText

**Severity Level: Error**

## Description

The use of the `AsPlainText` parameter with the `ConvertTo-SecureString` command can expose secure information.

## How

Use a standard encrypted variable to perform any SecureString conversions.

## Recommendations

If you do need an ability to retrieve the password from somewhere without prompting the user, consider using Windows Credential Store as used in the BetterCredentials module ( https://www.powershellgallery.com/packages/BetterCredentials/4.4/Content/BetterCredentials.psm1). If that does not work, consider using Azure KeyVault (https://azure.microsoft.com/en-us/services/key-vault/) or AWS KMS (https://aws.amazon.com/kms/).

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
