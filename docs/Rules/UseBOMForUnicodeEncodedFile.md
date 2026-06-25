---
description: Use BOM encoding for non-ASCII files
ms.date: 06/08/2026
ms.topic: reference
title: UseBOMForUnicodeEncodedFile
---
# UseBOMForUnicodeEncodedFile

**Severity Level: Warning**

## Description

This rule detects files that are encoded with Unicode or other non-ASCII formats but don't include a
Byte Order Mark (BOM).

When you save files with Unicode encoding, it's important to include a BOM so that applications can
properly interpret the file's encoding. This rule helps ensure that your PowerShell scripts and
other text files are saved with the appropriate BOM when using Unicode or similar non-ASCII
encodings.

### Using the Encoding parameter

When you use PowerShell commands that write to files, set the **Encoding** parameter to a value that
produces a BOM. In PowerShell 7 and later, these encoding values produce a BOM:

- `bigendianunicode`
- `bigendianutf32`
- `oem`
- `unicode`
- `utf32`
- `utf8BOM`

When you create a script file using a text editor, configure the editor to save the file with a BOM.
Consult your editor's documentation for instructions on how to save files with a BOM.

## Example

### Noncompliant

```powershell
# Writes non-ASCII text without a BOM.
Set-Content -Path .\script.ps1 -Value "Write-Output 'café'" -Encoding utf8NoBOM
```

### Compliant

```powershell
# Writes non-ASCII text with a BOM.
Set-Content -Path .\script.ps1 -Value "Write-Output 'café'" -Encoding utf8BOM
```

## See also

- [about_Character_Encoding](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_character_encoding)
- [Set-Content](https://learn.microsoft.com/powershell/module/microsoft.powershell.management/set-content)
- [Understanding file encoding in VS Code and PowerShell](https://learn.microsoft.com/powershell/scripting/dev-cross-plat/vscode/understanding-file-encoding)
