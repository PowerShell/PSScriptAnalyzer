/*
* Copyright (c) Microsoft Corporation. All rights reserved.
* Licensed under the MIT License.
*/
using System;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting;

namespace HostedAnalyzerTest
{
    public class Analyzer
    {
        HostedAnalyzer ha;
        public Analyzer() {
            ha = new HostedAnalyzer();
        }

        public AnalyzerResult Test1()
        {
            AnalyzerResult result = ha.Analyze("gcm");
            return result;
        }

        public AnalyzerResult Test2()
        {
            AnalyzerResult result = ha.Analyze("gcm|%{$_}");
            return result;
        }

        public AnalyzerResult Test3()
        {
            AnalyzerResult result = ha.Analyze("$a = gcm|%{$_}");
            return result;
        }

        public string Test4() {
            return ha.Fix("gci");
        }

        public string Test5() {
            return ha.Fix("gci|%{$_}");
        }

        public string Test6() {
            return ha.Format(ha.Fix("gci|%{$_}|?{$_}"));
        }

    }
}
