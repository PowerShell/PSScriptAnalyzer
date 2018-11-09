using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Types = Microsoft.PowerShell.CrossCompatibility.Data.Types;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class AssemblyNameData
    {
        private readonly Types.AssemblyNameData _assemblyNameData;

        private readonly Lazy<string> _fullName;

        private readonly Lazy<AssemblyName> _assemblyName;

        public AssemblyNameData(Types.AssemblyNameData assemblyNameData)
        {
            _assemblyNameData = assemblyNameData;
            _fullName = new Lazy<string>(GetFullName);
            _assemblyName = new Lazy<AssemblyName>(() => new AssemblyName(FullName));
        }

        public string Name => _assemblyNameData.Name;

        public Version Version => _assemblyNameData.Version;

        public string Culture => _assemblyNameData.Culture;

        public IReadOnlyList<byte> PublicKeyToken => _assemblyNameData.PublicKeyToken;

        public string FullName => _fullName.Value;

        public AssemblyName AsAssemblyName()
        {
            return _assemblyName.Value;
        }

        private string GetFullName()
        {
            var sb = new StringBuilder(Name);

            if (Version != null)
            {
                sb.Append(", Version=").Append(Version);
            }

            if (Culture != null)
            {
                sb.Append(", Culture=").Append(Culture);
            }

            if (PublicKeyToken != null)
            {
                string tokenHex = BitConverter.ToString(_assemblyNameData.PublicKeyToken)
                    .Replace("-", "");

                sb.Append(", PublicKeyToken=").Append(tokenHex);
            }

            return sb.ToString();
        }
    }
}