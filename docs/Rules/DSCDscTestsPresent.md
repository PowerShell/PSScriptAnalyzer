---
description: Dsc tests are present
ms.date: 06/28/2023
ms.topic: reference
title: DSCDscTestsPresent
---
# DscTestsPresent

**Severity Level: Information**

## Description

Checks that DSC tests for given resource are present.

## How

To fix a violation of this rule, please make sure `Tests` directory is present:

- For non-class based resources it should exist at the same folder level as `DSCResources` folder.
- For class based resources it should be present at the same folder level as resource `.psm1` file.

The `Tests` folder should contain test script for given resource. The filename should contain the
resource's name.

## Example

### Non-class based resource

Let's assume we have non-class based resource with a following file structure:

- xAzure
  - DSCResources
    - MSFT_xAzureSubscription
      - MSFT_xAzureSubscription.psm1
      - MSFT_xAzureSubscription.schema.mof

In this case, to fix this warning, we should add tests in a following way:

- xAzure
  - DSCResources
    - MSFT_xAzureSubscription
      - MSFT_xAzureSubscription.psm1
      - MSFT_xAzureSubscription.schema.mof
  - Tests
    - MSFT_xAzureSubscription_Tests.ps1

### Class based resource

Let's assume we have class based resource with a following file structure:

- MyDscResource
  - MyDscResource.psm1
  - MyDscResource.psd1

In this case, to fix this warning, we should add tests in a following way:

- MyDscResource
  - MyDscResource.psm1
  - MyDscResource.psd1
  - Tests
    - MyDscResource_Tests.ps1
