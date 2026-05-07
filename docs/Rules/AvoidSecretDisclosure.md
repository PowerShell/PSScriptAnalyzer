---
description: Avoid secret disclosure
ms.date: 05/03/2026
ms.topic: reference
title: AvoidSecretDisclosure
---
# AvoidSecretDisclosure

**Severity Level: Warning**

## Description

Disclosing a secret might result in security vulnerabilities such as memory trails or logging trails
that could be exploited by attackers. This rule identifies instances where a secret is being
converted to plain text, which can lead to unintended exposure of sensitive information.

> [!IMPORTANT]
> The general approach of dealing with credentials is to avoid them and instead rely on other means
> to authenticate, such as certificates or Windows authentication.

## How to Fix

In general, avoid any code pattern that involves converting secrets to plaintext or accessing
plaintext secrets.

- For `ConvertFrom-SecureString -AsPlainText`: Use `-Credential` parameter instead
- For `SecureStringTo*` methods: Avoid converting to plaintext
- For `Password` properties: Use secure credential objects directly or the SecureString equivalent
  `SecurePassword` instead of accessing plaintext passwords.

> [!NOTE]
> For custom properties named "Password", it is recommended to rename them to something that does
> not imply they contain secrets, or to ensure that they do not actually contain secrets. If
> renaming is not possible, consider suppressing the warning for those specific cases.

## Configuration

### Parameters

- `Enable` - Enables or disables the rule. Default value is `$false`.

## Example

### Wrong

```powershell
$credential = Get-Credential
$url = "https://server.contoso.com:8089/services/search/jobs/export"
$body = @{
    search = "search index=_internal | reverse | table index,host,source,sourceType,_raw"
    output_mode = "csv"
    earliest_time = "-2d@d"
    latest_time = "-1d@d"
    username = $credential.UserName
    password = $credential.GetNetworkCredential().Password
}
Invoke-RestMethod -Method 'Post' -Uri $url -Body $body -OutFile output.csv
```

### Correct

```powershell
$credential = Get-Credential
$url = "https://server.contoso.com:8089/services/search/jobs/export"
$body = @{
    search = "search index=_internal | reverse | table index,host,source,sourceType,_raw"
    output_mode = "csv"
    earliest_time = "-2d@d"
    latest_time = "-1d@d"
}
Invoke-RestMethod -Method 'Post' -Uri $url -Credential $credential -Body $body -OutFile output.csv
```
