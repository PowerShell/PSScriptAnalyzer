// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes a PowerShell runtime,
    /// as reported by $PSVersionTable.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PowerShellData : ICloneable
    {
        /// <summary>
        /// The self-reported version of PowerShell.
        /// From $PSVersionTable.PSVersion.
        /// </summary>
        [DataMember]
        public PowerShellVersion Version { get; set; }

        /// <summary>
        /// The edition of PowerShell, from
        /// $PSVersionTable.PSEdition.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Edition { get; set; }

        /// <summary>
        /// The compatible downlevel versions
        /// of PowerShell, from $PSVersionTable.PSCompatibleVersions.
        /// </summary>
        [DataMember]
        public Version[] CompatibleVersions { get; set; }

        /// <summary>
        /// The PowerShell remoting protocol version supported,
        /// from $PSVersionTable.PSRemotingProtocolVersion.
        /// </summary>
        [DataMember]
        public Version RemotingProtocolVersion { get; set; }
        
        /// <summary>
        /// The PowerShell serialization protocol version
        /// supported, from $PSVersionTable.SerializationVersion.
        /// </summary>
        [DataMember]
        public Version SerializationVersion { get; set; }

        /// <summary>
        /// The supported WSMan stack version, from
        /// $PSVersionTable.WSManStackVersion.
        /// </summary>
        [DataMember]
        public Version WSManStackVersion { get; set; }

        /// <summary>
        /// The git commit ID of the PowerShell runtime
        /// if it differs from the reported PowerShell version.
        /// From $PSVersionTable.GitCommitId.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string GitCommitId { get; set; }

        /// <summary>
        /// The machine architecture of the
        /// PowerShell runtime process.
        /// From System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture
        /// in .NET Core. Either X64 or X86 in .NET Framework.
        /// </summary>
        [DataMember]
        public Architecture ProcessArchitecture { get; set; }

        /// <summary>
        /// Create a deep clone of the PowerShell data object.
        /// </summary>
        public object Clone()
        {
            return new PowerShellData()
            {
                Edition = Edition,
                GitCommitId = GitCommitId,
                ProcessArchitecture = ProcessArchitecture,
                RemotingProtocolVersion = RemotingProtocolVersion,
                SerializationVersion = SerializationVersion,
                Version = Version,
                WSManStackVersion = WSManStackVersion,
                CompatibleVersions = (Version[])CompatibleVersions.Clone()
            };
        }
    }
}
