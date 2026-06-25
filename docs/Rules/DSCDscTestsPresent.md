---
description: DSC tests are present
ms.date: 06/03/2026
ms.topic: reference
title: DSCDscTestsPresent
---
# DscTestsPresent

**Severity Level: Information**

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
