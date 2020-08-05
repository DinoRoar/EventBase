namespace EventBase.Client.Spec
{
    public class TestEvent : Event
    {
        public string Name { get; }

        public TestEvent(string name)
        {
            Name = name;
        }
    }
}