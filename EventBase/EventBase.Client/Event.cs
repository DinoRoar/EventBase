using Newtonsoft.Json;

namespace EventBase.Client
{
    public class Event : HasId
    {
        public string Id { get; }

        public string EventType => this.GetType().Name;

        public Event()
        {
            Id = GetId();
        }

        [JsonConstructor]
        public Event(string id)
        {
            Id = id;
        }
    }
}