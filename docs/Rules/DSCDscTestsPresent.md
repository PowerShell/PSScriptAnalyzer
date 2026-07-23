---
description: DSC tests are present
ms.date: 07/21/2026
ms.topic: reference
title: DSCDscTestsPresent
---
# DscTestsPresent

**Severity Level: Information**

**Default state: Always enabled**

## Description

This rule detects if Desired State Configuration (DSC) tests for a given resource are present.

To fix a violation of this rule, you must ensure that the `Tests` directory is present for:

- Non-class based resources, it should exist at the same folder level as the `DSCResources`
  folder.
- Class based resources, it should be at the same folder level as the resource's `.psm1` file.

The `Tests` folder must contain a test script for the given resource. The filename should include
the resource's name.

## Example

### Non-class based resource

Let's assume we have non-class based resource with the following file structure:

- xAzure
  - DSCResources
    - MSFT_xAzureSubscription
      - MSFT_xAzureSubscription.psm1
      - MSFT_xAzureSubscription.schema.mof

In this case, to fix this warning, add tests in the following way:

- xAzure
  - DSCResources
    - MSFT_xAzureSubscription
      - MSFT_xAzureSubscription.psm1
      - MSFT_xAzureSubscription.schema.mof
  - Tests
    - MSFT_xAzureSubscription_Tests.ps1

### Class based resource

Let's assume we have class based resource with the following file structure:

- MyDscResource
  - MyDscResource.psm1
  - MyDscResource.psd1

In this case, to fix this warning, add tests in the following way:

- MyDscResource
  - MyDscResource.psm1
  - MyDscResource.psd1
  - Tests
    - MyDscResource_Tests.ps1

## Configure rule

This rule is always enabled and isn't configurable. Use one of the following methods to avoid using
this rule:

- Create a custom rule configuration file to include only the rules you want or exclude the rules
  you don't want.
- Add the appropriate rule suppression attributes to your code to suppress the rule for specific
  code blocks. For more information, see the _Suppressing rules_ section of [Using PSScriptAnalyzer][02].

<!-- link references -->
[02]: ../using-scriptanalyzer.md