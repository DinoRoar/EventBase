using System;

namespace EventBase.Client
{
    public class Projection
    {
        public string Name { get; }
        public Func<StreamEvent, string> StreamName { get; }
        public Predicate<StreamEvent> Predicate { get; }

        public Projection(string name, Func<StreamEvent, string> streamName, Predicate<StreamEvent> predicate)
        {
            Name = name;
            StreamName = streamName;
            Predicate = predicate;
        }
    }
}