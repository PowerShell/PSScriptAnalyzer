// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes an event on a .NET type.
    /// </summary>
    [Serializable]
    [DataContract]
    public class EventData : ICloneable
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

        /// <summary>
        /// Create a deep clone of the event data object.
        /// </summary>
        public object Clone()
        {
            return new EventData()
            {
                IsMulticast = IsMulticast,
                HandlerType = HandlerType
            };
        }
    }
}
