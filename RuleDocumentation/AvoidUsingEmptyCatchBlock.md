# AvoidUsingEmptyCatchBlock

**Severity Level: Warning**

## Description

Empty catch blocks are considered a poor design choice as they result in the errors occurring in a `try` block not being acted upon.

While this does not inherently lead to issues, they should be avoided wherever possible.

## How

Use ```Write-Error``` or ```throw``` statements within the catch block.

## Example

### Wrong

``` PowerShell
try
{
	1/0
}
catch [DivideByZeroException]
{
}
```

### Correct

``` PowerShell
try
{
	1/0
}
catch [DivideByZeroException]
{
	Write-Error "DivideByZeroException"
}

try
{
	1/0
}
catch [DivideByZeroException]
{
	Throw "DivideByZeroException"
}
```
