---
description: {{Brief description of the rule behavior}}
ms.date: {{MM/DD/YYYY}}
ms.topic: reference
title: {{RuleName}}
---
# {{RuleName}}

**Severity Level: {{Error | Warning | Information}}**

**Default state: {{Enabled | Disabled | Always enabled}}**

## Description

{{Explain what the rule detects, why the pattern is a problem, and the recommended
practice.}}

## {{Optional explanatory topic}}

{{Add a table, supported values, compatibility information, or remediation guidance
when the rule needs context before its examples. Remove this section when it doesn't
apply.}}

## Example

### Noncompliant

```powershell
{{Code that produces the diagnostic}}
```

### Compliant

```powershell
{{Equivalent code that doesn't produce the diagnostic}}
```

## Examples

### {{Scenario name}}

#### Noncompliant

```powershell
{{Code that produces the diagnostic for this scenario}}
```

#### Compliant

```powershell
{{Equivalent compliant code for this scenario}}
```

## Configure rule

{{For a configurable rule, use the following configuration and include the Parameters
section. For a nonconfigurable rule, replace this text with an explanation that the
rule isn't configurable and describe available exclusion or suppression options.}}

```powershell
@{
    Rules = @{
        PS{{RuleName}} = @{
            Enable = $true
            {SettingName} = {Value}
        }
    }
}
```

## Parameters

### {{SettingName}}

{{Explain what the setting controls, its accepted value type or values, and its
default value. Add another H3 section for each setting. Remove this section for a
nonconfigurable rule.}}

## Suppression

{{Explain any rule-specific suppression syntax or examples. Remove this section when
general suppression guidance linked from Configure rule is enough.}}

```powershell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PS{{RuleName}}', '')]
```

## Further reading

- {{[Article title][01]}}
- [Using PSScriptAnalyzer][02]

<!-- Link references -->
[01]: {{URL or absolute path}}
[02]: ../using-scriptanalyzer.md
