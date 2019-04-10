// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AsmNameDataMut = Microsoft.PowerShell.CrossCompatibility.Data.AssemblyNameData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// A readonly query object for .NET assembly name information.
    /// </summary>
    public class AssemblyNameData
    {
        private readonly AsmNameDataMut _assemblyNameData;

        private readonly Lazy<string> _fullName;

        private readonly Lazy<AssemblyName> _assemblyName;

        /// <summary>
        /// Create a query object for assembly name information from collected assembly name data.
        /// </summary>
        /// <param name="assemblyNameData">Collected assembly name data object.</param>
        public AssemblyNameData(AsmNameDataMut assemblyNameData)
        {
            _assemblyNameData = assemblyNameData;
            _fullName = new Lazy<string>(GetFullName);
            _assemblyName = new Lazy<AssemblyName>(() => new AssemblyName(FullName));
        }

        /// <summary>
        /// The simple name of the assembly.
        /// </summary>
        public string Name => _assemblyNameData.Name;

        /// <summary>
        /// The version of the assembly.
        /// </summary>
        public Version Version => _assemblyNameData.Version;

        /// <summary>
        /// The culture of the assembly, if it is not null, "" or "neutral".
        /// </summary>
        public string Culture => _assemblyNameData.Culture;

        /// <summary>
        /// The public key token of the assembly, if any.
        /// </summary>
        public IReadOnlyList<byte> PublicKeyToken => _assemblyNameData.PublicKeyToken;

        /// <summary>
        /// The full name of the assembly, in strong name format.
        /// </summary>
        public string FullName => _fullName.Value;

        /// <summary>
        /// Gets a System.Reflection.AssemblyName object from this assembly name.
        /// </summary>
        /// <returns></returns>
        public AssemblyName AsAssemblyName()
        {
            return _assemblyName.Value;
        }

        /// <summary>
        /// Builds a formatted assembly name from this assembly name object.
        /// </summary>
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
