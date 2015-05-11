#AvoidUsingConvertToSecureStringWithPlainTextNoViolations
**Severity Level: Error**


##Description

Information in the script should be protected properly. Using ConvertTo-SecureString with plain text will expose secure information.

##How to Fix

To fix a violation of this rule, please use a standard encrypted variable to do the conversion.

##Example

Wrong： 

```
$notsecure = convertto-securestring "abc" -asplaintext -force

New-Object System.Management.Automation.PSCredential -ArgumentList "username", (ConvertTo-SecureString "notsecure" -AsPlainText -Force)

```

Correct: 

```
$secure = read-host -assecurestring
$encrypted = convertfrom-securestring -securestring $secure
convertto-securestring -string $encrypted
```
