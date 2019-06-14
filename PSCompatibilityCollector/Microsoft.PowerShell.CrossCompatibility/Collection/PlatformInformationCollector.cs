// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.CrossCompatibility.Data;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using Microsoft.Win32;
using SMA = System.Management.Automation;

namespace Microsoft.PowerShell.CrossCompatibility.Collection
{
    /// <summary>
    /// Collects information about the current platform.
    /// </summary>
    public class PlatformInformationCollector : IDisposable
    {
        // Paths on Linux to search for key/value-paired information about the OS.
        private static readonly IReadOnlyCollection<string> s_releaseInfoPaths = new string[]
        {
            "/etc/lsb-release",
            "/etc/os-release",
        };

        private static readonly IReadOnlyList<string> s_distributionIdKeys = new string[]
        {
            "ID",
            "DISTRIB_ID"
        };

        private static readonly IReadOnlyList<string> s_distributionVersionKeys = new string[]
        {
            "VERSION_ID",
            "DISTRIB_RELEASE"
        };

        private static readonly IReadOnlyList<string> s_distributionPrettyNameKeys = new string[]
        {
            "PRETTY_NAME",
            "DISTRIB_DESCRIPTION"
        };

        private static readonly Regex s_macOSNameRegex = new Regex(
            @"System Version: (.*?)(\(|$)",
            RegexOptions.Multiline | RegexOptions.Compiled);


        /// <summary>
        /// Collect all release info files into a lookup table in memory.
        /// Overrides pre-existing keys if there are duplicates.
        /// </summary>
        /// <returns>A dictionary with the keys and values of all the release info files on the machine.</returns>
        public static IReadOnlyDictionary<string, string> GetLinuxReleaseInfo()
        {
            var releaseInfoKeyValueEntries = new Dictionary<string, string>();

            foreach (string path in s_releaseInfoPaths)
            {
                try
                {
                    using (FileStream fileStream = File.OpenRead(path))
                    using (var reader = new StreamReader(fileStream))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();

                            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                            {
                                continue;
                            }

                            string[] elements = line.Split('=');
                            releaseInfoKeyValueEntries[elements[0]] = Dequote(elements[1]);
                        }
                    }
                }
                catch (IOException)
                {
                    // Different Linux distributions have different /etc/*-release files.
                    // It's more efficient (and correct timing-wise) for us to try to read the file and catch the exception
                    // than to test for its existence and then open it.
                    //
                    // See:
                    //  - https://www.freedesktop.org/software/systemd/man/os-release.html
                    //  - https://gist.github.com/natefoo/814c5bf936922dad97ff

                    // Ignore that the file doesn't exist and just continue to the next one.
                }
            }

            return releaseInfoKeyValueEntries;
        }

        private readonly Lazy<Hashtable> _lazyPSVersionTable;

        private readonly Lazy<PowerShellVersion> _lazyPSVersion;

        private readonly Lazy<CurrentVersionInfo> _lazyCurrentVersionInfo;

        private readonly Lazy<Win32OSCimInfo> _lazyWin32OperatingSystemInfo;

        private SMA.PowerShell _pwsh;

        /// <summary>
        /// Create a new platform information collector around the given PowerShell session.
        /// </summary>
        /// <param name="pwsh">The PowerShell session to gather platform information from.</param>
        public PlatformInformationCollector(SMA.PowerShell pwsh)
        {
            _pwsh = pwsh;
            _lazyPSVersionTable = new Lazy<Hashtable>(() => _pwsh.AddScript("$PSVersionTable").InvokeAndClear<Hashtable>().FirstOrDefault());
            _lazyPSVersion = new Lazy<PowerShellVersion>(() => PowerShellVersion.Create(PSVersionTable["PSVersion"]));
            _lazyCurrentVersionInfo = new Lazy<CurrentVersionInfo>(() => ReadCurrentVersionFromRegistry());
            _lazyWin32OperatingSystemInfo = new Lazy<Win32OSCimInfo>(() => GetWin32OperatingSystemInfo());
        }

        private Hashtable PSVersionTable => _lazyPSVersionTable.Value;

        private CurrentVersionInfo RegistryCurrentVersionInfo => _lazyCurrentVersionInfo.Value;

        internal PowerShellVersion PSVersion => _lazyPSVersion.Value;

        /// <summary>
        /// Gets all platform information about the current platform.
        /// </summary>
        /// <returns>A platform data object with information about the platform.</returns>
        public PlatformData GetPlatformData()
        {
            return new PlatformData()
            {
                Dotnet = GetDotNetData(),
                OperatingSystem = GetOperatingSystemData(),
                PowerShell = GetPowerShellData(),
            };
        }

        /// <summary>
        /// Gets data on the .NET runtime this session is running on.
        /// </summary>
        /// <returns>A .NET data object about the current session.</returns>
        public DotnetData GetDotNetData()
        {
#if !CoreCLR
            return new DotnetData()
            {
                ClrVersion = Environment.Version,
                Runtime = DotnetRuntime.Framework
            };
#else
            return new DotnetData()
            {
                ClrVersion = Environment.Version, // TODO: This might be better recording the last part of RuntimeInformation.FrameworkDescription
                Runtime = RuntimeInformation.FrameworkDescription.StartsWith(".NET Core")
                    ? DotnetRuntime.Core
                    : DotnetRuntime.Framework,
            };
#endif
        }

        /// <summary>
        /// Get metadata about the current PowerShell runtime.
        /// </summary>
        /// <returns>A PowerShellData object detailing the current PowerShell runtime.</returns>
        public PowerShellData GetPowerShellData()
        {
            var psData = new PowerShellData()
            {
                CompatibleVersions = (Version[])PSVersionTable["PSCompatibleVersions"],
                Edition = (string)PSVersionTable["PSEdition"],
                RemotingProtocolVersion = (Version)PSVersionTable["PSRemotingProtocolVersion"],
                SerializationVersion = (Version)PSVersionTable["SerializationVersion"],
                Version = PSVersion,
                WSManStackVersion = (Version)PSVersionTable["WSManStackVersion"],
                ProcessArchitecture = GetProcessArchitecture()
            };

            string gitCommitId = (string)PSVersionTable["GitCommitId"];
            if (gitCommitId != psData.Version.ToString())
            {
                psData.GitCommitId = gitCommitId;
            }

            return psData;
        }

        /// <summary>
        /// Get information about the current operating system.
        /// </summary>
        /// <returns>A data object with metadata about the current operating system.</returns>
        public OperatingSystemData GetOperatingSystemData()
        {
            var osData = new OperatingSystemData()
            {
                Description = GetOSDescription(),
                Architecture = GetOSArchitecture(),
                Family = GetOSFamily(),
                Platform = GetOSPlatform(),
                Version = GetOSVersion(),
            };

            switch (osData.Family)
            {
                case OSFamily.Windows:
                    osData.Name = osData.Description;
                    if (!string.IsNullOrEmpty(Environment.OSVersion.ServicePack))
                    {
                        osData.ServicePack = Environment.OSVersion.ServicePack;
                    }
                    osData.SkuId = GetWinSkuId();
                    break;

                case OSFamily.Linux:
                    IReadOnlyDictionary<string, string> releaseInfo = GetLinuxReleaseInfo();

                    osData.DistributionId = GetEntryFromReleaseInfo(releaseInfo, s_distributionIdKeys);
                    osData.DistributionVersion = GetEntryFromReleaseInfo(releaseInfo, s_distributionVersionKeys);
                    osData.DistributionPrettyName = GetEntryFromReleaseInfo(releaseInfo, s_distributionPrettyNameKeys);
                    osData.Name = osData.DistributionPrettyName;
                    break;

                case OSFamily.MacOS:
                    osData.Name = GetMacOSName();
                    break;
            }

            return osData;
        }

        private string GetMacOSName()
        {
            try
            {
                using (var spProcess = new Process())
                {
                    spProcess.StartInfo.UseShellExecute = false;
                    spProcess.StartInfo.RedirectStandardOutput = true;
                    spProcess.StartInfo.CreateNoWindow = true;
                    spProcess.StartInfo.FileName = "/usr/sbin/system_profiler";
                    spProcess.StartInfo.Arguments = "SPSoftwareDataType";

                    spProcess.Start();
                    spProcess.WaitForExit();

                    string output = spProcess.StandardOutput.ReadToEnd();
                    return s_macOSNameRegex.Match(output).Groups[1].Value;
                }
            }
            catch
            {
                return null;
            }
        }

        private string GetOSDescription()
        {
#if CoreCLR
            // This key was introduced in PowerShell 6
            return (string)PSVersionTable["OS"];
#else
            if (_lazyWin32OperatingSystemInfo.IsValueCreated)
            {
                return _lazyWin32OperatingSystemInfo.Value.OSName;
            }

            return RegistryCurrentVersionInfo.ProductName;
#endif
        }

        private string GetOSVersion()
        {
#if CoreCLR
            // On Linux, we want to record the kernel branch, since this can differentiate Azure
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return File.ReadAllText("/proc/sys/kernel/osrelease");
            }
#endif
            return Environment.OSVersion.Version.ToString();
        }

        private string GetOSPlatform()
        {
#if CoreCLR
            if (PSVersion.Major >= 6)
            {
                return (string)PSVersionTable["Platform"];
            }
#endif
            return "Win32NT";
        }

        private OSFamily GetOSFamily()
        {
#if CoreCLR
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSFamily.Windows;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSFamily.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSFamily.MacOS;
            }

            return OSFamily.Other;
#else
            // .NET Framework only runs on Windows
            return OSFamily.Windows;
#endif
        }

        private Architecture GetProcessArchitecture()
        {
#if CoreCLR
            // Our Architecture enum is deliberately value-compatible
            return (Architecture)RuntimeInformation.ProcessArchitecture;
#else
            // We assume .NET Framework must be on an Intel architecture
            // net452 does not reliably have the above API
            return Environment.Is64BitProcess
                ? Architecture.X64
                : Architecture.X86;
#endif
        }

        private Architecture GetOSArchitecture()
        {
#if CoreCLR
            // Our Architecture enum is deliberately value-compatible
            return (Architecture)RuntimeInformation.OSArchitecture;
#else
            // We assume .NET Framework must be on an Intel architecture
            // net452 does not reliably have the above API
            return Environment.Is64BitOperatingSystem
                ? Architecture.X64
                : Architecture.X86;
#endif
        }

        private DotnetRuntime GetDotnetRuntime()
        {
#if CoreCLR
            // Our CoreCLR is actuall .NET Standard, so we could be loaded into net47
            return RuntimeInformation.FrameworkDescription.StartsWith(".NET Core")
                ? DotnetRuntime.Core
                : DotnetRuntime.Framework;
#else
            return DotnetRuntime.Framework;
#endif
        }

        /// <summary>
        /// Get the Windows SKU ID of the current PowerShell session.
        /// </summary>
        /// <returns>An unsigned 32-bit integer representing the SKU of the current Windows OS.</returns>
        private uint GetWinSkuId()
        {
            // There are a few ways to get this, with varying support on different systems.
            // So we try them in order from least to most expensive.

            // If we have a cached value here, try this first
            if (_lazyWin32OperatingSystemInfo.IsValueCreated
                && _lazyWin32OperatingSystemInfo.Value != null
                && _lazyWin32OperatingSystemInfo.Value.SkuID != 0)
            {
                return _lazyWin32OperatingSystemInfo.Value.SkuID;
            }

            // If we don't have to deal with service pack details, try a GetProductInfo P/Invoke next
            if (string.IsNullOrEmpty(Environment.OSVersion.ServicePack)
                && GetProductInfo(
                    Environment.OSVersion.Version.Major,
                    Environment.OSVersion.Version.Minor,
                    0,
                    0,
                    out uint skuId)
                && skuId != 0)
            {
                return skuId;
            }

            // Try looking in the registry
            // The SKU enum we define matches up all the names to the IDs
            if (Enum.TryParse(RegistryCurrentVersionInfo.EditionID, out WindowsSku sku)
                && sku != WindowsSku.Undefined)
            {
                return (uint)sku;
            }

            // Finally try CIM
            if (_lazyWin32OperatingSystemInfo.Value != null
                && _lazyWin32OperatingSystemInfo.Value.SkuID != 0)
            {
                return _lazyWin32OperatingSystemInfo.Value.SkuID;
            }

            // Admit defeat
            return (uint)WindowsSku.Undefined;
        }

        private static CurrentVersionInfo ReadCurrentVersionFromRegistry()
        {
            using (RegistryKey currentVersion = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                return new CurrentVersionInfo(
                    editionId: (string)currentVersion.GetValue("EditionID"),
                    productName: ((string)currentVersion.GetValue("ProductName"))?.TrimEnd());
            }
        }

        private static string GetEntryFromReleaseInfo(IReadOnlyDictionary<string, string> releaseInfo, IEnumerable<string> possibleKeys)
        {
            foreach (string key in possibleKeys)
            {
                if (releaseInfo.TryGetValue(key, out string entry))
                {
                    return entry;
                }
            }

            return null;
        }

        private static Win32OSCimInfo GetWin32OperatingSystemInfo()
        {
            try
            {
                // Creating the session with null here prevents use of the network layer
                using (var cimSession = CimSession.Create(null))
                {
                    CimInstance win32OSInstance = cimSession.EnumerateInstances(@"root\cimv2", "Win32_OperatingSystem")
                        .FirstOrDefault();

                    if (win32OSInstance == null)
                    {
                        return null;
                    }

                    return new Win32OSCimInfo(
                        osName: ((string)win32OSInstance.CimInstanceProperties["Name"].Value)?.Split('|')[0].TrimEnd(),
                        skuID: (uint)win32OSInstance.CimInstanceProperties["OperatingSystemSku"].Value);
                }
            }
            catch
            {
                return null;
            }
        }

        private static string Dequote(string s)
        {
            var sb = new StringBuilder(s.Length);
            bool isEscaped = false;
            QuoteState quoteState = QuoteState.None;
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"':
                        if (isEscaped || quoteState == QuoteState.Single)
                        {
                            sb.Append(c);
                            break;
                        }
                        quoteState ^= QuoteState.Double;
                        break;

                    case '\'':
                        if (isEscaped || quoteState == QuoteState.Double)
                        {
                            sb.Append(c);
                            break;
                        }
                        quoteState ^= QuoteState.Single;
                        break;

                    case '\\':
                        if (isEscaped)
                        {
                            sb.Append(c);
                            break;
                        }

                        isEscaped = true;
                        // Continue here so we don't immediately unset
                        continue;

                    default:
                        sb.Append(c);
                        break;
                }

                isEscaped = false;
            }

            return sb.ToString();
        }

        [DllImport("kernel32.dll")]
        private static extern bool GetProductInfo(
            int dwOSMajorVersion,
            int dwOSMinorVersion,
            int dwSpMajorVersion,
            int dwSpMinorVersion,
            out uint pdwReturnedProductType);


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _pwsh.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

        private enum QuoteState
        {
            None = 0,
            Single = 1,
            Double = 2,
        }

        private class CurrentVersionInfo
        {
            public CurrentVersionInfo(string editionId, string productName)
            {
                EditionID = editionId;
                ProductName = productName;
            }

            public string EditionID { get; }

            public string ProductName { get; }
        }

        private class Win32OSCimInfo
        {
            public Win32OSCimInfo(string osName, uint skuID)
            {
                OSName = osName;
                SkuID = skuID;
            }

            public string OSName { get; }

            public uint SkuID { get; }
        }
    }
}