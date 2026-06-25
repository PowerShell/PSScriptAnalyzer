---
description: DSC examples are present
ms.date: 06/03/2026
ms.topic: reference
title: DSCDscExamplesPresent
---
# DscExamplesPresent

**Severity Level: Information**

## Description

This rule detects if Desired State Configuration (DSC) examples for a given resource are present.

To fix a violation of this rule, you must ensure that the `Examples` directory exists for:

- Non-class based resources, it should be at the same folder level as the `DSCResources` folder.
- Class based resources, it should be at the same folder level as the resource's `.psm1` file.

The `Examples` folder must contain a sample configuration for the resource. The filename should
include the resource's name.

## Example

### Non-class based resource

Let's assume we have non-class based resource with the following file structure:

- xAzure
  - DSCResources
    - MSFT_xAzureSubscription
      - MSFT_xAzureSubscription.psm1
      - MSFT_xAzureSubscription.schema.mof

In this case, to fix this warning, add examples in the following way:

- xAzure
  - DSCResources
    - MSFT_xAzureSubscription
      - MSFT_xAzureSubscription.psm1
      - MSFT_xAzureSubscription.schema.mof
  - Examples
    - MSFT_xAzureSubscription_AddSubscriptionExample.ps1
    - MSFT_xAzureSubscription_RemoveSubscriptionExample.ps1

### Class based resource

Let's assume we have class based resource with the following file structure:

- MyDscResource
  - MyDscResource.psm1
  - MyDscResource.psd1

In this case, to fix this warning, add examples in the following way:

- MyDscResource
  - MyDscResource.psm1
  - MyDscResource.psd1
  - Examples
    - MyDscResource_Example1.ps1
    - MyDscResource_Example2.ps1
