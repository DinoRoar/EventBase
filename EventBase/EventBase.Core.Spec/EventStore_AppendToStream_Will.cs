using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EventBase.Core.Spec
{
    public class InMemoryEventStoreWill : EventStore_Will<InMemoryPersistence>
    {
        public InMemoryEventStoreWill() : base(new InMemoryPersistence())
        {

        }
    }

   public abstract class EventStore_Will<T> where T : IEventPersistence
    {
        private readonly T _persistence;

        protected EventStore_Will(T persistenceLayer)
        {
            _persistence = persistenceLayer;
        }

        [Fact]
        public async Task ThrowUnexpectedPositionWhenStreamPositionIncorrectOnAppend()
        {
            await Assert.ThrowsAsync<StreamDoesNotExistException>(() =>
                new EventStore(_persistence).Append("streamName", 5, new byte[0], new byte[0]));
        }

        [Fact]
        public async Task WriteEventWhenExpectedPositionIsAnyOnAppend()
        {
            var eventPersistence = _persistence;
            var underTest = new EventStore(eventPersistence);
            var result = await underTest.Append("streamName", EventStore.StreamPositions.Any, new byte[0], new byte[0]);
            Assert.Equal(0, result.NextPosition);

            var eventsTotal =  await _persistence.GetAllEvents().CountAsync();
            Assert.Equal(1, eventsTotal);
        }

        [Fact]
        public async Task WriteEventWhenStreamNotCreatedAndPositionIsCreate()
        {
            var eventPersistence = _persistence;
            var underTest = new EventStore(eventPersistence);
            var result = await underTest.Append("streamName", EventStore.StreamPositions.Create, new byte[0], new byte[0]);
            Assert.Equal(1, result.NextPosition);
        }

        [Fact]
        public async Task ThrowUnexpectedPositionWhenStreamExistsAndPositionWrongOnAppend()
        {
            const string streamName = "streamName";
            await _persistence.AppendEvent(new IEventPersistence.Event(streamName,0, new byte[0], new byte[0]));

            var underTest = new EventStore(_persistence);

            await Assert.ThrowsAsync<UnexpectedStreamPositionException>(() =>
                underTest.Append(streamName, 2, new byte[0], new byte[0]));
        }



        [Fact]
        public async Task ReturnAllEventsInThePersistence()
        {
            const string streamName = "streamName";
            var underTest = new EventStore(_persistence);

            await underTest.Append(streamName, EventStore.StreamPositions.Any, new byte[0], new byte[0]);
            await underTest.Append(streamName, EventStore.StreamPositions.Any, new byte[0], new byte[0]);
            await underTest.Append(streamName, EventStore.StreamPositions.Any, new byte[0], new byte[0]);

            var events = _persistence.GetAllEvents();
            var total = await events.CountAsync();

            Assert.Equal(3, total);
        }

        [Fact]
        public async Task ReturnEventsByStream()
        {
            var underTest = new EventStore(_persistence);

            await underTest.Append("streamName", EventStore.StreamPositions.Any, new byte[0], new byte[0]);
            await underTest.Append("streamName2", EventStore.StreamPositions.Any, new byte[0], new byte[0]);
            await underTest.Append("streamName2", EventStore.StreamPositions.Any, new byte[0], new byte[0]);

            var events = underTest.ReadStreamForward("streamName");
            var total = await events.CountAsync();
            Assert.Equal(1, total);

            events = underTest.ReadStreamForward("streamName2");
            total = await events.CountAsync();
            Assert.Equal(2, total);
        }
    }
}
