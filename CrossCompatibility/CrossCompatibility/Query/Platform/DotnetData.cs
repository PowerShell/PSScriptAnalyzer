using System;
using DotnetRuntime = Microsoft.PowerShell.CrossCompatibility.Data.Platform.DotnetRuntime;
using DotnetDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Platform.DotnetData;

namespace Microsoft.PowerShell.CrossCompatibility.Query.Platform
{
    public class DotnetData
    {
        private readonly DotnetDataMut _dotnetData;

        public DotnetData(DotnetDataMut dotnetData)
        {
            _dotnetData = dotnetData;
        }

        public Version ClrVersion => _dotnetData.ClrVersion;

        public DotnetRuntime Runtime => _dotnetData.Runtime;
    }
}