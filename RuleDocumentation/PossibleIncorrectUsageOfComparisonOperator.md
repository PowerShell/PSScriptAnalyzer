# PossibleIncorrectUsageOfComparisonOperator

**Severity Level: Information**

## Description

In many programming languages, the comparison operator for greater is `>` but `PowerShell` uses `-gt` for it and `-ge` for `>=`. Similarly the equality operator is denoted as `==` or `=` in many programming languages, but `PowerShell` uses `-eq`. Since using as the FileRedirection operator `>` or the assignment operator are rarely needed inside if statements, this rule wants to call out this case because it was probably unintentional.
