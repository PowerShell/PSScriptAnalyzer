# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.
param(
    [ValidateSet("PSGallery", "CFS")]
    [string]$PSRepository = "PSGallery"
)

if ($PSRepository -eq "CFS" -and -not (Get-PSResourceRepository -Name CFS -ErrorAction SilentlyContinue)) {
    Register-PSResourceRepository -Name CFS -Uri "https://pkgs.dev.azure.com/powershell/PowerShell/_packaging/powershell/nuget/v3/index.json"
}

Install-PSResource -Repository $PSRepository -TrustRepository -Name platyPS
Install-PSResource -Repository $PSRepository -TrustRepository -Name Pester
