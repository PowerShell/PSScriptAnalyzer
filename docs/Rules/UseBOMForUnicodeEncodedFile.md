---
description: Use BOM encoding for non-ASCII files
ms.date: 01/07/2025
ms.topic: reference
title: UseBOMForUnicodeEncodedFile
---
# UseBOMForUnicodeEncodedFile

**Severity Level: Warning**

## Description

For a file encoded with a format other than ASCII, ensure Byte Order Mark (BOM) is present to ensure
that any application consuming this file can interpret it correctly.

You can use this rule to test any arbitrary text file, but the intent is to ensure that PowerShell
scripts are saved with a BOM when using a Unicode encoding.

## How

For PowerShell commands that write to files, ensure that you set the encoding parameter to a value
that produces a BOM. In PowerShell 7 and higher, the following values of the **Encoding** parameter
produce a BOM:

- `bigendianunicode`
- `bigendianutf32`
- `oem`
- `unicode`
- `utf32`
- `utf8BOM`

When you create a script file using a text editor, ensure that the editor is configured to save the
file with a BOM. Consult the documentation for your text editor for instructions on how to save
files with a BOM.

## Further reading

For more information, see the following articles:

- [about_Character_Encoding](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_character_encoding)
- [Set-Content](https://learn.microsoft.com/powershell/module/microsoft.powershell.management/set-content)
- [Understanding file encoding in VS Code and PowerShell](https://learn.microsoft.com/powershell/scripting/dev-cross-plat/vscode/understanding-file-encoding)
