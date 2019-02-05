// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using TypeDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.TypeData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class TypeData
    {
        public TypeData(string name, TypeDataMut typeData)
        {
            Name = name;
            Instance = typeData.Instance == null ? null : new MemberData(typeData.Instance);
            Static = typeData.Static == null ? null : new MemberData(typeData.Static);
        }

        public string Name { get; }

        public MemberData Instance { get; }

        public MemberData Static { get; }
    }
}