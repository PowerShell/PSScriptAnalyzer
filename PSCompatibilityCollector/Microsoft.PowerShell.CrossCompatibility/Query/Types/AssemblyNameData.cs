// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Lazy<string> _fullName;

        private readonly Lazy<AssemblyName> _assemblyName;

        /// <summary>
        /// Create a query object for assembly name information from collected assembly name data.
        /// </summary>
        /// <param name="assemblyNameData">Collected assembly name data object.</param>
        public AssemblyNameData(AsmNameDataMut assemblyNameData)
        {
            Name = assemblyNameData.Name;
            Version = assemblyNameData.Version;
            Culture = assemblyNameData.Culture;
            PublicKeyToken = assemblyNameData.PublicKeyToken;
            _fullName = new Lazy<string>(GetFullName);
            _assemblyName = new Lazy<AssemblyName>(() => new AssemblyName(FullName));
        }

        /// <summary>
        /// The simple name of the assembly.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The version of the assembly.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// The culture of the assembly, if it is not null, "" or "neutral".
        /// </summary>
        public string Culture { get; }

        /// <summary>
        /// The public key token of the assembly, if any.
        /// </summary>
        public IReadOnlyList<byte> PublicKeyToken { get; }

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
                var publicKeyTokenArray = new byte[PublicKeyToken.Count];
                for (int i = 0; i < publicKeyTokenArray.Length; i++)
                {
                    publicKeyTokenArray[i] = PublicKeyToken[i];
                }

                string tokenHex = BitConverter.ToString(publicKeyTokenArray)
                    .Replace("-", "");

                sb.Append(", PublicKeyToken=").Append(tokenHex);
            }

            return sb.ToString();
        }
    }
}
