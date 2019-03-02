// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using EventDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.EventData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for .NET event members on types.
    /// </summary>
    public class EventData
    {
        private readonly EventDataMut _eventData;

        /// <summary>
        /// Create a new query object around collected .NET event information.
        /// </summary>
        /// <param name="name">The name of the event member.</param>
        /// <param name="eventData">The collected event data.</param>
        public EventData(string name, EventDataMut eventData)
        {
            Name = name;
            _eventData = eventData;
        }

        /// <summary>
        /// The name of the event member.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// True if the event is multicast, false otherwise.
        /// </summary>
        public bool IsMulticast => _eventData.IsMulticast;

        /// <summary>
        /// The type of the handler for this event.
        /// </summary>
        public string HandlerType => _eventData.HandlerType;
    }
}
