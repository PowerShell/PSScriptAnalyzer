---
name: Bug report 🐛
about: Report errors or unexpected behavior 🤔

---

Before submitting a bug report:

- Make sure you are able to repro it on the latest released version
- Perform a quick search for existing issues to check if this bug has already been reported

Steps to reproduce
------------------

```PowerShell

```

Expected behavior
-----------------

```none

```

Actual behavior
---------------

```none

```

If an unexpected error was thrown then please report the full error details using e.g. `$error[0] | Select-Object *`

Environment data
----------------

<!-- Provide the output of the following 2 commands -->

```PowerShell
> $PSVersionTable

> (Get-Module -ListAvailable PSScriptAnalyzer).Version | ForEach-Object { $_.ToString() }

```
