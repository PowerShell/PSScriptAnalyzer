#AvoidInvokingEmptyMembers
**Severity Level: Warning**


##Description

Invoking non-constant members would cause potential bugs. Please double check the syntax to make sure members invoked are non-constant.


##How to Fix

To fix a violation of this rule, please provide requested members for given types or classes.

##Example

Wrong：    

    "abc".('len'+'gth')

Correct:    

    "abc".('length')