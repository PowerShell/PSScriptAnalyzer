using System;
using OperatingSystemDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Platform.OperatingSystemData;

namespace Microsoft.PowerShell.CrossCompatibility.Query.Platform
{
    public class OperatingSystemData
    {
        private readonly OperatingSystemDataMut _operatingSystemData;

        public OperatingSystemData(OperatingSystemDataMut operatingSystemData)
        {
            _operatingSystemData = operatingSystemData;
        }

        public string Name => _operatingSystemData.Name;

        public string Platform => _operatingSystemData.Platform;

        public Architecture Architecture => _operatingSystemData.Architecture;

        public OSFamily Family => _operatingSystemData.Family;

        public string Version => _operatingSystemData.Version;

        public string ServicePack => _operatingSystemData.ServicePack;

        public uint? SkuId => _operatingSystemData.SkuId;

        public string DistributionId => _operatingSystemData.DistributionId;

        public string DistirbutionVersion => _operatingSystemData.DistributionVersion;

        public string DistributionPrettyName => _operatingSystemData.DistributionPrettyName;
    }
}