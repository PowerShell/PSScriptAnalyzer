// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using DotnetRuntime = Microsoft.PowerShell.CrossCompatibility.Data.DotnetRuntime;
using DotnetDataMut = Microsoft.PowerShell.CrossCompatibility.Data.DotnetData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for .NET runtime data.
    /// </summary>
    public class DotnetData
    {
        /// <summary>
        /// Create a new query object around a .NET data instance.
        /// </summary>
        /// <param name="dotnetData">The .NET data to be queried.</param>
        public DotnetData(DotnetDataMut dotnetData)
        {
            ClrVersion = dotnetData.ClrVersion;
            Runtime = dotnetData.Runtime;
        }

        /// <summary>
        /// The version of the .NET CLR used by the PowerShell platform.
        /// </summary>
        public Version ClrVersion { get; }

        /// <summary>
        /// The edition of the .NET runtime PowerShell is running on.
        /// </summary>
        public DotnetRuntime Runtime { get; }
    }
}
