---
description: Use identical mandatory parameters for DSC Get, Set, and Test TargetResource functions in a resource
ms.date: 06/03/2026
ms.topic: reference
title: DSCUseIdenticalMandatoryParametersForDSC
---
# UseIdenticalMandatoryParametersForDSC

**Severity Level: Error**

## Description

This rule detects if MOF-based Desired State Configuration (DSC) resources have properties
declared with `Key` or `Required` attributes in a `.mof` file that aren't present as mandatory
parameters in the corresponding functions. These properties must be declared as mandatory
parameters in the `Get-TargetResource`, `Set-TargetResource`, and `Test-TargetResource` functions.

All properties with `Key` and `Required` attributes should have matching mandatory
parameters in the **Get**, **Set**, and **Test** functions.

## Example

Consider the following MOF schema file.

```mof
class WaitForAny : OMI_BaseResource
{
    [key, Description("Name of Resource on remote machine")]
    string Name;

    [required, Description("List of remote machines")]
    string NodeName[];
};
```

### Noncompliant

```powershell
function Get-TargetResource
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Message
    )
}

function Set-TargetResource
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Message,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Name
    )
}

function Test-TargetResource
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Message

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Name
    )
}
```

### Compliant

```powershell
function Get-TargetResource
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Message,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Name
    )
}

function Set-TargetResource
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Message,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Name
    )
}

function Test-TargetResource
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Message,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [String]
        $Name
    )
}
```
