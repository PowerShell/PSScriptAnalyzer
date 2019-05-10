# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

# Add the relevant binary module
if ($PSVersionTable.PSVersion.Major -ge 6)
{
    Import-Module ([System.IO.Path]::Combine($PSScriptRoot, 'netstandard2.0', 'Microsoft.PowerShell.CrossCompatibility.dll')) -Force
}
else
{
    Import-Module ([System.IO.Path]::Combine($PSScriptRoot, 'net452', 'Microsoft.PowerShell.CrossCompatibility.dll')) -Force
}
