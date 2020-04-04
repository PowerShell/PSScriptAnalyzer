// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using OperatingSystemDataMut = Microsoft.PowerShell.CrossCompatibility.Data.OperatingSystemData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for platform operating system information.
    /// </summary>
    public class OperatingSystemData
    {
        /// <summary>
        /// Create an operating system query object from operating system information.
        /// </summary>
        /// <param name="operatingSystemData">Operating system data object from a profile.</param>
        public OperatingSystemData(OperatingSystemDataMut operatingSystemData)
        {
            Name = operatingSystemData.Name;
            Description = operatingSystemData.Description;
            Platform = operatingSystemData.Platform;
            Architecture = operatingSystemData.Architecture;
            Family = operatingSystemData.Family;
            Version = operatingSystemData.Version;
            ServicePack = operatingSystemData.ServicePack;
            SkuId = operatingSystemData.SkuId;
            DistributionId = operatingSystemData.DistributionId;
            DistributionVersion = operatingSystemData.DistributionVersion;
            DistributionPrettyName = operatingSystemData.DistributionPrettyName;
        }

        /// <summary>
        /// The name of the operating system.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The description of the operating system as reported by $PSVersionTable.OS.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The name of the platform as reported by $PSVersionTable.Platform.
        /// </summary>
        public string Platform { get; }

        /// <summary>
        /// The OS machine architecture, from System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.
        /// </summary>
        public Architecture Architecture { get; }

        /// <summary>
        /// Specifies whether the OS is Windows, Linux or macOS.
        /// </summary>
        public OSFamily Family { get; }

        /// <summary>
        /// The self declared version of the operating system (kernel on Linux).
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// The Windows Service Pack of the OS, if any.
        /// </summary>
        public string ServicePack { get; }

        /// <summary>
        /// The Windows SKU ID of the OS, if any.
        /// </summary>
        public uint? SkuId { get; }

        /// <summary>
        /// The Linux distribution ID of the OS, if any.
        /// </summary>
        public string DistributionId { get; }

        /// <summary>
        /// The version of the Linux distribution, if any.
        /// </summary>
        public string DistributionVersion { get; }

        /// <summary>
        /// The self-reported "pretty name" of the Linux distribution, if any.
        /// </summary>
        public string DistributionPrettyName { get; }

        /// <summary>
        /// The human-readable name of this operating system
        /// </summary>
        public string FriendlyName => Family == OSFamily.Linux ? DistributionPrettyName : Name;

        /// <summary>
        /// A descriptive enum form of the Windows SKU, if one is available.
        /// </summary>
        public WindowsSku? Sku
        {
            get
            {
                // Type inference fails on a ternary, so we are forced to write this out...
                if (SkuId.HasValue) { return (WindowsSku)SkuId.Value; }
                return null;
            }
        }
    }
}
