namespace EventBase.Client
{
    public class EventTypeProjection : Projection
    {
        public EventTypeProjection()
            : base("EventType", BuildStreamName, e => !(e is ProjectedEvent))
        {

        }

        private static string BuildStreamName(StreamEvent arg)
        {
            var eventType = arg.Event.Type;
            return $"et-{eventType}";
        }
    }
}