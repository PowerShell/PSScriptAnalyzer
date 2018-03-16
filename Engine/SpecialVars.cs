// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    internal class SpecialVars
    {
        internal static object ThisVariable = new object();
        internal const string @foreach = "foreach";
        internal const string @switch = "switch";
        internal const string Question = "?";

        internal const string Underbar = "_";
        internal const string Args = "args";
        internal const string This = "this";
        internal const string Input = "input";
        internal const string PSCmdlet = "PSCmdlet";
        internal const string PSBoundParameters = "PSBoundParameters";
        internal const string MyInvocation = "MyInvocation";
        internal const string PSScriptRoot = "PSScriptRoot";
        internal const string PSCommandPath = "PSCommandPath";
        internal const string ExecutionContext = "ExecutionContext";
        internal const string Matches = "Matches";
        internal const string PSVersionTable = "PSVersionTable";
        internal const string OFS = "OFS";

        internal static readonly string[] InitializedVariables;

        static SpecialVars()
        {
            InitializedVariables = new string[]
                                    {
                                        @foreach,
                                        @switch,
                                        Question
                                    };

            InitializedVariables = InitializedVariables.Concat(
                AutomaticVariables.Concat(
                PreferenceVariables.Concat(
                OtherInitializedVariables))).ToArray();
        }

        internal static readonly string[] AutomaticVariables = new string[]  
                                                               {  
                                                                   Underbar,  
                                                                   Args,  
                                                                   This,  
                                                                   Input,  
                                                                   PSCmdlet,  
                                                                   PSBoundParameters,  
                                                                   MyInvocation,  
                                                                   PSScriptRoot,  
                                                                   PSCommandPath,
                                                                   ExecutionContext,
                                                                   Matches,
                                                                   PSVersionTable,
                                                                   OFS
                                                               };
        internal static readonly Type[] AutomaticVariableTypes = new Type[]  
                                                                 {  
                                                                   /* Underbar */          typeof(object),  
                                                                   /* Args */              typeof(object[]),  
                                                                   /* This */              typeof(object),  
                                                                   /* Input */             typeof(object),  
                                                                   /* PSCmdlet */          typeof(PSCmdlet),  
                                                                   /* PSBoundParameters */ typeof(Dictionary<string,object>),  
                                                                   /* MyInvocation */      typeof(InvocationInfo),  
                                                                   /* PSScriptRoot */      typeof(string),  
                                                                   /* PSCommandPath */     typeof(string),  
                                                                   /* ExecutionContext */  typeof(EngineIntrinsics),
                                                                   /* Matches */           typeof(System.Collections.Hashtable),
                                                                   /* PSVersionTable */    typeof(System.Collections.Hashtable),
                                                                   /* OFS */               typeof(object)
                                                                 };


        internal const string DebugPreference = "DebugPreference";
        internal const string VerbosePreference = "VerbosePreference";
        internal const string ErrorActionPreference = "ErrorActionPreference";
        internal const string WhatIfPreference = "WhatIfPreference";
        internal const string WarningPreference = "WarningPreference";
        internal const string ConfirmPreference = "ConfirmPreference";
        internal const string ProgressPreference = "ProgressPreference";
        internal const string InformationPreference = "InformationPreference";

        internal static readonly string[] PreferenceVariables = new string[]
                                                                {
                                                                    DebugPreference,
                                                                    VerbosePreference,
                                                                    ErrorActionPreference,
                                                                    WhatIfPreference,
                                                                    WarningPreference,
                                                                    ConfirmPreference,
                                                                    ProgressPreference,
                                                                    InformationPreference
                                                                };

        internal static readonly Type[] PreferenceVariableTypes = new Type[]
                                                                {
                                                                    /* DebugPreference */   typeof(ActionPreference),
                                                                    /* VerbosePreference */ typeof(ActionPreference),
                                                                    /* ErrorPreference */   typeof(ActionPreference),
                                                                    /* WhatIfPreference */  typeof(SwitchParameter),
                                                                    /* WarningPreference */ typeof(ActionPreference),
                                                                    /* ConfirmPreference */ typeof(ConfirmImpact),
                                                                    /* ProgressPreference */ typeof(Enum),
                                                                    /* InformationPreference */ typeof(ActionPreference),
                                                                };

        internal enum AutomaticVariable
        {
            Underbar = 0,
            Args = 1,
            This = 2,
            Input = 3,
            PSCmdlet = 4,
            PSBoundParameters = 5,
            MyInvocation = 6,
            PSScriptRoot = 7,
            PSCommandPath = 8,
            NumberOfAutomaticVariables // 1 + the last, used to initialize global scope.
        }

        internal enum PreferenceVariable
        {
            Debug = 9,
            Verbose = 10,
            Error = 11,
            WhatIf = 12,
            Warning = 13,
            Confirm = 14,
        }

        internal const string Host = "Host";
        internal const string HistorySize = "MaximumHistoryCount";
        internal const string OutputEncoding = "OutputEncoding";
        internal const string NestedPromptLevel = "NestedPromptLevel";
        internal const string StackTrace = "StackTrace";
        internal const string FirstToken = "^";
        internal const string LastToken = "$";
        internal const string PSItem = "PSItem";  // simple alias for $_
        internal const string Error = "error";
        internal const string EventError = "error";
        internal const string PathExt = "env:PATHEXT";
        internal const string PSEmailServer = "PSEmailServer";
        internal const string PSDefaultParameterValues = "PSDefaultParameterValues";
        internal const string PSModuleAutoLoadingPreference = "PSModuleAutoLoadingPreference";
        internal const string pwd = "PWD";
        internal const string Null = "null";
        internal const string True = "true";
        internal const string False = "false";
        internal const string LastExitCode = "LastExitCode";

        internal static readonly string[] OtherInitializedVariables = new string[]
                                                                {
                                                                    Host,
                                                                    HistorySize,
                                                                    OutputEncoding,
                                                                    NestedPromptLevel,
                                                                    StackTrace,
                                                                    FirstToken,
                                                                    LastToken,
                                                                    PSItem,
                                                                    Error,
                                                                    EventError,
                                                                    PathExt,
                                                                    PSEmailServer,
                                                                    PSDefaultParameterValues,
                                                                    PSModuleAutoLoadingPreference,
                                                                    pwd,
                                                                    Null,
                                                                    True,
                                                                    False,
                                                                    LastExitCode
                                                                };

    }
}
