using System;
using System.Collections.Generic;
using PowerShellDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Platform.PowerShellData;

namespace Microsoft.PowerShell.CrossCompatibility.Query.Platform
{
    public class PowerShellData
    {
        private readonly PowerShellDataMut _powerShellData;

        public PowerShellData(PowerShellDataMut powerShellData)
        {
            _powerShellData = powerShellData;
        }

        public Version Version => _powerShellData.Version;

        public string Edition => _powerShellData.Edition;

        public IReadOnlyList<Version> CompatibleVersions => _powerShellData.CompatibleVersions;

        public Version RemotingProtocolVersion => _powerShellData.RemotingProtocolVersion;

        public Version SerializationVersion => _powerShellData.SerializationVersion;

        public Version WsManStackVersion => _powerShellData.WSManStackVersion;

        public string GitCommitId => _powerShellData.GitCommitId;

        public Architecture ProcessArchitecture => _powerShellData.ProcessArchitecture;
    }
}