class MyType
{
    static [MyType] CreateFromString([string]$Str)
    {
        return [MyType]::new($Str)
    }

    MyType([string]$Str)
    {
        $this.String = $Str
    }

    [string]$String

    [string]GetString()
    {
        return $this.String
    }
}

$stack = [System.Collections.Generic.Stack[string]]::new()
$add = 'Push'
foreach ($s in 'a','b','c','d')
{
    $stack.$add($s)
}

$create = 'CreateFromString'

while ($stack.Count -gt 0)
{
    $t = [MyType]::$create($stack.Pop())
    Write-Information $t.GetString()
}

Get-EventLog -LogName System | ogv

Invoke-WebRequest -Uri 'https://aka.ms/everyonebehappy/' -NoProxy -SkipHeaderValidation

$modSpec = $null
if (-not [Microsoft.PowerShell.Commands.ModuleSpecification]::TryParse("@{ ModuleName = 'Microsoft.PowerShell.Utility'; ModuleVersion = '3.0' }", [ref]$modSpec))
{
    throw "Module specification did not parse"
}

$m = Import-Module -FullyQualifiedName $modSpec -PassThru

return $m