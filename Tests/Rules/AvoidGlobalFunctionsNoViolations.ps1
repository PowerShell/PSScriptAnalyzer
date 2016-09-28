
function functionName {} 
{
	New-Alias -Name Name -Scope `
	Script -Value Value

	nal -Name Name -Scope "Script" -Value Value
}


New-Alias -Name Name -scope:Script -Value Value
nal -Name Name -scope:Script -Value Value
