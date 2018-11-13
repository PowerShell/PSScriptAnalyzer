using EventDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.EventData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class EventData
    {
        private readonly EventDataMut _eventData;

        public EventData(string name, EventDataMut eventData)
        {
            Name = name;
            _eventData = eventData;
        }

        public string Name { get; }

        public bool IsMulticast => _eventData.IsMulticast;

        public string HandlerType => _eventData.HandlerType;
    }
}