using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.CrossCompatibility.Data.Platform;
using Microsoft.PowerShell.CrossCompatibility.Query;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using Microsoft.Win32;
using SMA = System.Management.Automation;

namespace Microsoft.PowerShell.CrossCompatibility.Collection
{
    public class PlatformInformationCollector : IDisposable
    {
        private static readonly IReadOnlyCollection<string> s_releaseInfoPaths = new string[]
        {
            "/etc/lsb-release",
            "/etc/os-release",
        };

        private readonly Lazy<Hashtable> _lazyPSVersionTable;

        private readonly Lazy<PowerShellVersion> _lazyPSVersion;

        private readonly Lazy<CurrentVersionInfo> _lazyCurrentVersionInfo;

        private readonly Lazy<Win32OSCimInfo> _lazyWin32OperatingSystemInfo;

        private SMA.PowerShell _pwsh;

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

        public PlatformData GetPlatformData()
        {
            return new PlatformData()
            {
                Dotnet = GetDotNetData(),
                OperatingSystem = GetOperatingSystemData(),
                PowerShell = GetPowerShellData(),
            };
        }

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

        public OperatingSystemData GetOperatingSystemData()
        {
            var osData = new OperatingSystemData()
            {
                Architecture = GetOSArchitecture(),
                Family = GetOSFamily(),
                Name = GetOSName(),
                Platform = GetOSPlatform(),
                Version = GetOSVersion(),
            };

            switch (osData.Family)
            {
                case OSFamily.Windows:
                    if (!string.IsNullOrEmpty(Environment.OSVersion.ServicePack))
                    {
                        osData.ServicePack = Environment.OSVersion.ServicePack;
                    }
                    osData.SkuId = GetSkuId();
                    break;

                case OSFamily.Linux:
                    IReadOnlyDictionary<string, string> lsbInfo = GetLinuxReleaseInfo();
                    osData.DistributionId = lsbInfo["DistributionId"];
                    osData.DistributionVersion = lsbInfo["DistributionVersion"];
                    osData.DistributionPrettyName = lsbInfo["DistributionPrettyName"];
                    break;
            }

            return osData;
        }

        public IReadOnlyDictionary<string, string> GetLinuxReleaseInfo()
        {
            var dict = new Dictionary<string, string>();

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
                            dict[elements[0]] = Dequote(elements[1]);
                        }
                    }
                }
                catch (IOException)
                {
                    // Do nothing - just continue
                }
            }

            return dict;
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
            return OSFamily.Windows;
#endif
        }

        private Architecture GetProcessArchitecture()
        {
#if CoreCLR
            return (Architecture)RuntimeInformation.ProcessArchitecture;
#else
            return Environment.Is64BitProcess
                ? Architecture.X64
                : Architecture.X86;
#endif
        }

        private Architecture GetOSArchitecture()
        {
#if CoreCLR
            return (Architecture)RuntimeInformation.OSArchitecture;
#else
            return Environment.Is64BitOperatingSystem
                ? Architecture.X64
                : Architecture.X86;
#endif
        }

        private DotnetRuntime GetDotnetRuntime()
        {
#if CoreCLR
            return RuntimeInformation.FrameworkDescription.StartsWith(".NET Core")
                ? DotnetRuntime.Core
                : DotnetRuntime.Framework;
#else
            return DotnetRuntime.Framework;
#endif
        }

        private uint GetSkuId()
        {
            // If we have a cached value here, try this first
            if (_lazyWin32OperatingSystemInfo.IsValueCreated
                && _lazyWin32OperatingSystemInfo.Value != null
                && _lazyWin32OperatingSystemInfo.Value.SkuID != 0)
            {
                return _lazyWin32OperatingSystemInfo.Value.SkuID;
            }

            // If we don't have to deal with service pack details, try a GetProductInfo P/Invoke next
            if (string.IsNullOrEmpty(Environment.OSVersion.ServicePack) && GetProductInfo(
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
            if (Enum.TryParse(RegistryCurrentVersionInfo.EditionID, out WindowsSku sku)
                && sku != WindowsSku.Undefined)
            {
                return (uint)sku;
            }

            // Try definitely running CIM
            if (_lazyWin32OperatingSystemInfo.Value != null
                && _lazyWin32OperatingSystemInfo.Value.SkuID != 0)
            {
                return _lazyWin32OperatingSystemInfo.Value.SkuID;
            }

            // Admit defeat
            return (uint)WindowsSku.Undefined;
        }

        private string GetOSName()
        {
#if CoreCLR
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

        private static CurrentVersionInfo ReadCurrentVersionFromRegistry()
        {
            using (RegistryKey currentVersion = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                return new CurrentVersionInfo(
                    editionId: (string)currentVersion.GetValue("EditionID"),
                    productName: ((string)currentVersion.GetValue("ProductName"))?.TrimEnd());
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool GetProductInfo(
            int dwOSMajorVersion,
            int dwOSMinorVersion,
            int dwSpMajorVersion,
            int dwSpMinorVersion,
            out uint pdwReturnedProductType);

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