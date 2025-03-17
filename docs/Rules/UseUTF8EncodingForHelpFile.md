---
description: Use UTF8 Encoding For Help File
ms.date: 01/07/2025
ms.topic: reference
title: UseUTF8EncodingForHelpFile
---
# UseUTF8EncodingForHelpFile

**Severity Level: Warning**

## Description

Check that an `about_` help file uses UTF-8 encoding. The filename must start with `about_` and end
with `.help.txt`. The rule uses the **CurrentEncoding** property of the **StreamReader** class to
determine the encoding of the file.

## How

For PowerShell commands that write to files, ensure that you set the encoding parameter to `utf8`,
`utf8BOM`, or `utf8NoBOM`.

When you create a help file using a text editor, ensure that the editor is configured to save the
file in a UTF8 format. Consult the documentation for your text editor for instructions on how to
save files with a specific encoding.

## Further reading

For more information, see the following articles:

- [System.IO.StreamReader](https://learn.microsoft.com/dotnet/api/system.io.streamreader.currentencoding)
- [about_Character_Encoding](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_character_encoding)
- [Set-Content](https://learn.microsoft.com/powershell/module/microsoft.powershell.management/set-content)
- [Understanding file encoding in VS Code and PowerShell](https://learn.microsoft.com/powershell/scripting/dev-cross-plat/vscode/understanding-file-encoding)
