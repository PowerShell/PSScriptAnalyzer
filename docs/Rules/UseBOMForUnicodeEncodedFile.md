---
description: Use BOM encoding for non-ASCII files
ms.custom: PSSA v1.20.0
ms.date: 10/18/2021
ms.topic: reference
title: UseBOMForUnicodeEncodedFile
---
# UseBOMForUnicodeEncodedFile

**Severity Level: Warning**

## Description

For a file encoded with a format other than ASCII, ensure Byte Order Mark (BOM) is present to ensure
that any application consuming this file can interpret it correctly.

## How

Ensure that the file is encoded with BOM present.
