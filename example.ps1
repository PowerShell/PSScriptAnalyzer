using module Microsoft.PowerShell.Utility

class MyClass
{
  Print()
  {
    Write-Output "Hello"
  }
}

$x = [System.Text.StringBuilder]::new()
[void]$x.Append("Hi")
Write-Output $x.ToString()
