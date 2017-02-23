# AvoidInvokingEmptyMembers

**Severity Level: Warning**

## Description

Invoking non-constant members can cause potential bugs. Please double check the syntax to make sure that invoked members are constants.

## How

Provide the requested members for a given type or class.

## Example

### Wrong

``` PowerShell
$MyString = "abc"
$MyString.('len'+'gth')
```

### Correct

``` PowerShell
$MyString = "abc"
$MyString.('length')
```
