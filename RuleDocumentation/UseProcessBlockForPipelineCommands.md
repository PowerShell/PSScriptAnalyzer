# UseProcessBlockForPipelineCommands

**Severity Level: Warning**

## Description

Functions that support pipeline input should always handle parameter input in a process block. Unexpected behavior can result if input is handled directly in the body of a function where parameters declare pipeline support.

## Example

### Wrong

``` PowerShell
Function Get-Number
{
	[CmdletBinding()]
	Param(
		[Parameter(ValueFromPipeline)]
		[int]
		$Number
	)
	
	$Number
}
```

#### Result

```
PS C:\> 1..5 | Broken
5
```

### Correct

``` PowerShell
Function Get-Number
{
	[CmdletBinding()]
	Param(
		[Parameter(ValueFromPipeline)]
		[int]
		$Number
	)
	
	process
	{
		$Number
	}
}
```

#### Result

```
PS C:\> 1..5 | Correct
1
2
3
4
5
```
