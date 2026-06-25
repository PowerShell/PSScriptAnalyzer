---
description: Use UTF8 encoding for help file
ms.date: 06/11/2026
ms.topic: reference
title: UseUTF8EncodingForHelpFile
---
# UseUTF8EncodingForHelpFile

**Severity Level: Warning**

## Description

This rule detects when an `about_` help file doesn't use UTF-8 encoding. The rule verifies that help
files starting with `about_` and ending with `.help.txt` are saved in UTF-8 encoding. It uses the
**CurrentEncoding** property of the **StreamReader** class to check the file's encoding.

To ensure your help files use the correct encoding, follow these guidelines:

- When using PowerShell commands to write to files, set the encoding parameter to `utf8`, `utf8BOM`,
  or `utf8NoBOM`.
- When creating or editing help files in a text editor, configure it to save files in UTF-8 format.
  Consult your editor's documentation for instructions on setting the file encoding.

## Example

### Noncompliant

```powershell
$helpText = @'
TOPIC
  about_Contoso

SHORT DESCRIPTION
  Describes Contoso commands.
'@

$helpText | Set-Content -Path "about_Contoso.help.txt" -Encoding Unicode
```

### Compliant

```powershell
$helpText = @'
TOPIC
  about_Contoso

SHORT DESCRIPTION
  Describes Contoso commands.
'@

$helpText | Set-Content -Path "about_Contoso.help.txt" -Encoding utf8NoBOM
```

## See also

For more information, see the following articles:

- [System.IO.StreamReader](https://learn.microsoft.com/dotnet/api/system.io.streamreader.currentencoding)
- [about_Character_Encoding](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_character_encoding)
- [Set-Content](https://learn.microsoft.com/powershell/module/microsoft.powershell.management/set-content)
- [Understanding file encoding in VS Code and PowerShell](https://learn.microsoft.com/powershell/scripting/dev-cross-plat/vscode/understanding-file-encoding)
