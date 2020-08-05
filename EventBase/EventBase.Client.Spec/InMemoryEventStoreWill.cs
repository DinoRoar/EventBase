using System.Linq;
using Xunit;

namespace EventBase.Client.Spec
{
    public class InMemoryEventStoreWill
    {
        [Fact]
        public void ReadAndWriteEvent()
        {
            var ut = new InMemoryEventStore();

            var streamName = "streamName";
            ut.Append(new CreateStreamEvent(streamName, StreamPositions.Any, new TestEvent("name")));
            var events = ut.ReadStream(streamName);
            Assert.Single(events);

            var e = events.First();
            Assert.IsType<TestEvent>(e.Event);
            var actualEvent = (TestEvent) e.Event;
            Assert.Equal("name", actualEvent.Name);
        }

        [Fact]
        public void WriteEventReadFromCategory()
        {
            var ut = new InMemoryEventStore();

            var streamName = "streamName";
            ut.Append(new CreateStreamEvent(streamName, StreamPositions.Any, new TestEvent("name")));
            var events = ut.ReadStream("ca-streamName");
            Assert.Single(events);

            var e = events.First().GetOriginatingEvent;
            Assert.IsType<TestEvent>(e);
            var actualEvent = (TestEvent)e;
            Assert.Equal("name", actualEvent.Name);
        }

        [Fact]
        public void WriteEventReadFromEventType()
        {
            var ut = new InMemoryEventStore();

            var streamName = "streamName";
            ut.Append(new CreateStreamEvent(streamName, StreamPositions.Any, new TestEvent("name")));
            var events = ut.ReadStream("et-TestEvent");
            Assert.Single(events);

            var e = events.First().GetOriginatingEvent;
            Assert.IsType<TestEvent>(e);
            var actualEvent = (TestEvent)e;
            Assert.Equal("name", actualEvent.Name);
        }
    }
}