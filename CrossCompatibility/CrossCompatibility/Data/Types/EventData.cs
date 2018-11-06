using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Types
{
    /// <summary>
    /// Describes an event on a .NET type.
    /// </summary>
    [Serializable]
    [DataContract]
    public class EventData
    {
        /// <summary>
        /// True if the event is a multicast event, false otherwise.
        /// </summary>
        [DataMember]
        public bool IsMulticast { get; set; }

        /// <summary>
        /// The type of the handler required for this event.
        /// </summary>
        [DataMember]
        public string HandlerType { get; set; }
    }
}