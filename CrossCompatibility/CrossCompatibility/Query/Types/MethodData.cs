using System.Collections.Generic;
using MethodDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.MethodData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class MethodData
    {
        private readonly MethodDataMut _methodData;

        public MethodData(string name, MethodDataMut methodData)
        {
            Name = name;
            _methodData = methodData;
        }

        public string Name { get; }

        public string ReturnType => _methodData.ReturnType;

        public IReadOnlyList<IReadOnlyList<string>> OverloadParameters => _methodData.OverloadParameters;
    }
}