// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PowerShellDataMut = Microsoft.PowerShell.CrossCompatibility.Data.PowerShellData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for PowerShell installation information.
    /// </summary>
    public class PowerShellData
    {
        /// <summary>
        /// Create a new query object around collected PowerShell installation information.
        /// </summary>
        /// <param name="powerShellData">A PowerShell installation data object.</param>
        public PowerShellData(PowerShellDataMut powerShellData)
        {
            Version = powerShellData.Version;
            Edition = powerShellData.Edition;
            CompatibleVersions = new List<Version>(powerShellData.CompatibleVersions);
            RemotingProtocolVersion = powerShellData.RemotingProtocolVersion;
            SerializationVersion = powerShellData.SerializationVersion;
            WsManStackVersion = powerShellData.WSManStackVersion;
            GitCommitId = powerShellData.GitCommitId;
            ProcessArchitecture = powerShellData.ProcessArchitecture;
        }

        /// <summary>
        /// The version of PowerShell, as reported by $PSVersionTable.PSVersion.
        /// </summary>
        public PowerShellVersion Version { get; }

        /// <summary>
        /// The edition of PowerShell, as reported by $PSVersionTable.PSEdition.
        /// </summary>
        public string Edition { get; }

        /// <summary>
        /// Output of $PSVersionTable.PSCompatibleVersions.
        /// </summary>
        public IReadOnlyList<Version> CompatibleVersions { get; }

        /// <summary>
        /// Output of $PSVersionTable.PSRemotingProtocolVersion.
        /// </summary>
        public Version RemotingProtocolVersion { get; }

        /// <summary>
        /// Output of $PSVersionTable.SerializationVersion.
        /// </summary>
        public Version SerializationVersion { get; }

        /// <summary>
        /// Output of $PSVersionTable.WSManStackVersion.
        /// </summary>
        public Version WsManStackVersion { get; }

        /// <summary>
        /// Output of $PSVersionTable.GitCommitId, if it differs from the version.
        /// </summary>
        public string GitCommitId { get; }

        /// <summary>
        /// The self-reported process architecture from System.InteropServices.RuntimeInformation.ProcessArchitecture.
        /// </summary>
        public Architecture ProcessArchitecture { get; }
    }
}
