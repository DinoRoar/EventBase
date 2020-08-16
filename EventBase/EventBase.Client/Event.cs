using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace EventBase.Client
{
    public class Event : Message
    {
        public Event()
        {
        }

        [JsonConstructor]
        public Event(string id):base(id)
        {
        }
    }

    public class Message : HasId
    {
        public string Id { get; }

        public string Type => this.GetType().Name;

        public Message()
        {
            Id = GetId();
        }

        [JsonConstructor]
        public Message(string id)
        {
            Id = id;
        }
    }
}