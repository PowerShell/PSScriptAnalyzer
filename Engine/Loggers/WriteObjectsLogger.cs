// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands;
#if !CORECLR
using System.ComponentModel.Composition;
#endif // CORECLR
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Loggers
{
    /// <summary>
    /// WriteObjectsLogger: Logs Diagnostics though WriteObject.
    /// </summary>
#if !CORECLR
    [Export(typeof(ILogger))]
#endif
    public class WriteObjectsLogger : ILogger
    {
        #region Private members

#if CORECLR
        private CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
        private ResourceManager rm = new ResourceManager(
            "Microsoft.Windows.PowerShell.ScriptAnalyzer.Strings",
            typeof(WriteObjectsLogger).GetTypeInfo().Assembly);
#else
        private CultureInfo cul = Thread.CurrentThread.CurrentCulture;
        private ResourceManager rm = new ResourceManager("Microsoft.Windows.PowerShell.ScriptAnalyzer.Strings",
                                                                  Assembly.GetExecutingAssembly());
#endif

        #endregion

        #region Methods

        /// <summary>
        /// LogObject: Logs the given object though WriteObject.
        /// </summary>
        /// <param name="obj">The object to be logged</param>
        /// <param name="command">The Invoke-PSLint command this logger is running through</param>
        public void LogObject(Object obj, InvokeScriptAnalyzerCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("diagnostic");
            }
            command.WriteObject(obj);
        }

        /// <summary>
        /// GetName: Retrieves the name of this logger.
        /// </summary>
        /// <returns>The name of this logger</returns>
        public string GetName()
        {
            return rm.GetString("DefaultLoggerName", cul);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this logger.
        /// </summary>
        /// <returns>The description of this logger</returns>
        public string GetDescription()
        {
            return rm.GetString("DefaultLoggerDescription", cul);
        }

        #endregion
    }
}
