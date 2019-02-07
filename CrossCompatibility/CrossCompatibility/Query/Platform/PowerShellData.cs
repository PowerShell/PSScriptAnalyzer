// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using PowerShellDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Platform.PowerShellData;

namespace Microsoft.PowerShell.CrossCompatibility.Query.Platform
{
    /// <summary>
    /// Readonly query object for PowerShell installation information.
    /// </summary>
    public class PowerShellData
    {
        private readonly PowerShellDataMut _powerShellData;

        /// <summary>
        /// Create a new query object around collected PowerShell installation information.
        /// </summary>
        /// <param name="powerShellData">A PowerShell installation data object.</param>
        public PowerShellData(PowerShellDataMut powerShellData)
        {
            _powerShellData = powerShellData;
        }

        /// <summary>
        /// The version of PowerShell, as reported by $PSVersionTable.PSVersion.
        /// </summary>
        public PowerShellVersion Version => _powerShellData.Version;

        /// <summary>
        /// The edition of PowerShell, as reported by $PSVersionTable.PSEdition.
        /// </summary>
        public string Edition => _powerShellData.Edition;

        /// <summary>
        /// Output of $PSVersionTable.PSCompatibleVersions.
        /// </summary>
        public IReadOnlyList<Version> CompatibleVersions => _powerShellData.CompatibleVersions;

        /// <summary>
        /// Output of $PSVersionTable.PSRemotingProtocolVersion.
        /// </summary>
        public Version RemotingProtocolVersion => _powerShellData.RemotingProtocolVersion;

        /// <summary>
        /// Output of $PSVersionTable.SerializationVersion.
        /// </summary>
        public Version SerializationVersion => _powerShellData.SerializationVersion;

        /// <summary>
        /// Output of $PSVersionTable.WSManStackVersion.
        /// </summary>
        public Version WsManStackVersion => _powerShellData.WSManStackVersion;

        /// <summary>
        /// Output of $PSVersionTable.GitCommitId, if it differs from the version.
        /// </summary>
        public string GitCommitId => _powerShellData.GitCommitId;

        /// <summary>
        /// The self-reported process architecture from System.InteropServices.RuntimeInformation.ProcessArchitecture.
        /// </summary>
        public Architecture ProcessArchitecture => _powerShellData.ProcessArchitecture;
    }
}