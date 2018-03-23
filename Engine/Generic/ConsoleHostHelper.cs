// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Management.Automation.Host;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    internal static class ConsoleHostHelper
    {
        internal static void DisplayMessageUsingSystemProperties(PSHost psHost, string foregroundColorPropertyName, string backgroundPropertyName, string message)
        {
            var gotForegroundColor = TryGetPrivateDataConsoleColor(psHost, foregroundColorPropertyName, out ConsoleColor foregroundColor);
            var gotBackgroundColor = TryGetPrivateDataConsoleColor(psHost, backgroundPropertyName, out ConsoleColor backgroundColor);
            if (gotForegroundColor && gotBackgroundColor)
            {
                psHost.UI.WriteLine(foregroundColor: foregroundColor, backgroundColor: backgroundColor, value: message);
            }
            else
            {
                psHost.UI.WriteLine(message);
            }
        }

        private static bool TryGetPrivateDataConsoleColor(PSHost psHost, string propertyName, out ConsoleColor consoleColor)
        {
            consoleColor = default(ConsoleColor);
            var property = psHost.PrivateData.Properties[propertyName];
            if (property == null)
            {
                return false;
            }

            try
            {
                consoleColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), property.Value.ToString(), true);
            }
            catch (InvalidCastException)
            {
                return false;
            }

            return true;
        }
    }
}
