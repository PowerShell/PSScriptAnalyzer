using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Modules
{
    /// <summary>
    /// Describes a PowerShell cmdlet from a module.
    /// </summary>
    [Serializable]
    [DataContract]
    public class CmdletData : CommandData
    {
    }
}
