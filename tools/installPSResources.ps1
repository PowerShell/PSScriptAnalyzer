# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

$ErrorActionPreference = 'Stop'

Set-PSRepository -Name PSGallery -InstallationPolicy Trusted | Out-Null
if ($PSVersionTable.PSVersion.Major -lt 6) {
    throw "The build script requires PowerShell 7!"
}

# TODO: Switch to Install-PSResource when CI uses PowerShell 7.4
Install-Module -Name platyPS -Scope CurrentUser
Install-Module -Name Pester -Scope CurrentUser
