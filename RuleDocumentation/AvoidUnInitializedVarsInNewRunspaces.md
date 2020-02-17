# AvoidUnInitializedVarsInNewRunspaces

**Severity Level: Warning**

## Description

If a scriptblock is intended to be run as a new runspace, variables inside it should use $using: directive, or be initialized within the scriptblock.

## How to Fix

Within `Foreach-Object -Parallel {}`, instead of just using a variable from the parent scope, you have to use the `using:` directive:

## Example

### Wrong

``````PowerShell
$var = "foo"
1..2 | ForEach-Object -Parallel { $var }
``````

### Correct

``````PowerShell
$var = "foo"
1..2 | ForEach-Object -Parallel { $using:var }
``````