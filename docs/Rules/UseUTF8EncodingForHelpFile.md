---
description: Use UTF8 encoding for help file
ms.date: 07/21/2026
ms.topic: reference
title: UseUTF8EncodingForHelpFile
---
# UseUTF8EncodingForHelpFile

**Severity Level: Warning**

**Default state: Always enabled**

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

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][01].

## See also

For more information, see the following articles:

- [System.IO.StreamReader][02]
- [about_Character_Encoding][03]
- [Set-Content][04]
- [Understanding file encoding in Visual Studio Code and PowerShell][05]

<!-- link references -->
[01]: ../using-scriptanalyzer.md
[02]: /dotnet/api/system.io.streamreader.currentencoding
[03]: /powershell/module/microsoft.powershell.core/about/about_character_encoding
[04]: /powershell/module/microsoft.powershell.management/set-content
[05]: /powershell/scripting/dev-cross-plat/vscode/understanding-file-encoding
