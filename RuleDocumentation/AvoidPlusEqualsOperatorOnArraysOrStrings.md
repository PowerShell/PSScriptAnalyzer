# AvoidPlusEqualsOperatorOnArraysOrStrings

**Severity Level: Warning**

## Description

PowerShell's `+=` operator creates a new array everytime an element gets added to it. This can leads to a noticable performance hit on arrays that have more than 1000 elements and many elements are being added.

## How

Using a .Net list or builder type that supports methods for adding elements to it such as e.g. `System.Collections.ArrayList`, `System.Text.StringBuilder` or `[System.Collections.Generic.List[CustomType]]`.

## Example

### Wrong

``` PowerShell
$array = @()
foreach($i in 1..1000) {
    $array += Get-SecretSauce $i
}

```

``` PowerShell
$arrayList = New-Object System.Collections.ArrayList
foreach($i in 1..1000) {
    $arrayList += Get-SecretSauce $i
}
```

``` PowerShell
$message = "Let's start to enumerate a lot:"
foreach($i in 1..1000) {
    $message += "$i;"
}
```

### Correct

``` PowerShell
$array = New-Object System.Collections.ArrayList
foreach($i in 1..1000) {
    $array.Add(Get-SecretSauce $i)
}
```

``` PowerShell
$stringBuilder = New-Object System.Text.StringBuilder
$null = $stringBuilder.Append("Let's start to enumerate a lot:")
foreach($i in 1..1000) {
    $message += "$i;"
    $null = $stringBuilder.Append("$i;")
}
$message = $stringBuilder.ToString()
```
