# PossibleIncorrectUsageOfAssignmentOperator

**Severity Level: Information**

## Description

In many programming languages, the equality operator is denoted as `==` or `=`, but `PowerShell` uses `-eq`. Since assignment inside if statements are very rare, this rule wants to call out this case because it might have been unintentional.
