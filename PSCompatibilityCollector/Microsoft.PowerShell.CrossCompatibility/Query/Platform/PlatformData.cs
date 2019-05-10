// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using PlatformDataMut = Microsoft.PowerShell.CrossCompatibility.Data.PlatformData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for PowerShell platform data,
    /// describing the whole environment that PowerShell runs on top of.
    /// </summary>
    public class PlatformData
    {
        /// <summary>
        /// Create a new platform query object from collected platform data.
        /// </summary>
        /// <param name="platformData">The collected platform data, from the profile module.</param>
        public PlatformData(PlatformDataMut platformData)
        {
            if (platformData != null)
            {
                Dotnet = new DotnetData(platformData.Dotnet);
                OperatingSystem = new OperatingSystemData(platformData.OperatingSystem);
                PowerShell = new PowerShellData(platformData.PowerShell);
            }
        }

        /// <summary>
        /// Information about the .NET runtime PowerShell is running on.
        /// </summary>
        public DotnetData Dotnet { get; }

        /// <summary>
        /// Information about the OS PowerShell is running on.
        /// </summary>
        public OperatingSystemData OperatingSystem { get; }

        /// <summary>
        /// Information about the PowerShell installation itself.
        /// </summary>
        public PowerShellData PowerShell { get; }
    }
}
