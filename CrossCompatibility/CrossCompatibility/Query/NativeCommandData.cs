using System;
using NativeCommandDataMut = Microsoft.PowerShell.CrossCompatibility.Data.NativeCommandData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class NativeCommandData
    {
        public NativeCommandData(string name, NativeCommandDataMut nativeCommandMut)
        {
            Name = name;
            Version = nativeCommandMut?.Version;
            Path = nativeCommandMut?.Path;
        }

        public string Name { get; }

        public Version Version { get; }

        public string Path { get; }
    }
}