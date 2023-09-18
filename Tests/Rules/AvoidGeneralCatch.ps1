# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

try
{
    1/0
}
catch [DivideByZeroException]
{
    "catch divide by zero exception"
}
catch [System.Management.Automation.RuntimeException]
{
    "catch RuntimeException"
}
catch
{
    "No exception"
}
finally
{
    "cleaning up ..."
}