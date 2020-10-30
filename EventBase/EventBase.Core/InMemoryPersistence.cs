using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EventBase.Core
{
    public class InMemoryPersistence : IEventPersistence
    {
        /// <summary>
        /// Not used but in a file based approach each list block has at most int.max
        ///
        /// Just to give a flavor
        /// </summary>
        public long _eventIndexOffset = 0; // 

        public long _globalPosition = 0;

        private readonly List<PersistedEvent> _events = new List<PersistedEvent>();

        private readonly Dictionary<string, List<IndexedStreamEvent>> _streams = new Dictionary<string, List<IndexedStreamEvent>>();

        public async IAsyncEnumerable<IEventPersistence.Event> GetAllEvents()
        {
            foreach (var @event in _events)
            {
                yield return @event.ToEvent();
            }
        }

        public Task<IEventPersistence.AppendEventResult> AppendEvent(IEventPersistence.Event @event)
        {
            var streamName = @event.StreamName;
            long actualStreamPosition = 0;
            if (!_streams.ContainsKey(streamName))
            {
                _streams.Add(streamName, new List<IndexedStreamEvent>());
            }
            else
            {
                actualStreamPosition = _streams[streamName].Last().StreamPosition;
            }

            var persistedEvent = new PersistedEvent(_globalPosition, actualStreamPosition, @event);

            _events.Add(persistedEvent);
            _streams[streamName].Add(new IndexedStreamEvent(streamName, actualStreamPosition, _globalPosition ));
            _globalPosition += 1;

            return Task.FromResult(new IEventPersistence.AppendEventResult(actualStreamPosition));
        }

        public Task<IEventPersistence.GetStreamPositionResult> GetNextStreamPosition(string streamName)
        {
            if (!_streams.ContainsKey(streamName))
                return Task.FromResult(new IEventPersistence.GetStreamPositionResult());

            return Task.FromResult(new IEventPersistence.GetStreamPositionResult(_streams[streamName].Count));
        }

        public async IAsyncEnumerable<IEventPersistence.Event> ReadStreamForward(string streamName)
        {
            foreach (var streamEvent in _streams[streamName])
            {
                var persistedEvent = _events[Convert.ToInt32(streamEvent.GlobalEventPosition)];
                yield return streamEvent.ToEvent(persistedEvent);
            }
        }

        public class IndexedStreamEvent
        {
            public IndexedStreamEvent()
            {

            }

            public IndexedStreamEvent(string streamName, long streamPosition, long globalEventPosition)
            {
                StreamName = streamName;
                StreamPosition = streamPosition;
                GlobalEventPosition = globalEventPosition;
            }

            public long StreamPosition { get; set; }
            public long GlobalEventPosition { get; set; }
            public string StreamName { get; set; }

            public IEventPersistence.Event ToEvent(PersistedEvent persistedEvent)
            {
                return new IEventPersistence.Event(StreamName, StreamPosition, persistedEvent.EventData, persistedEvent.EventMetadata);
            }
        }

        public class PersistedEvent
        {
            public long GlobalPosition { get; set; }
            public long StreamPosition { get; set; }
            public byte[] EventData { get; set; }
            public byte[] EventMetadata { get; set; }

            public PersistedEvent()
            {
            }

            public PersistedEvent(in long globalPosition, in long streamPosition, IEventPersistence.Event @event)
            {
                GlobalPosition = globalPosition;
                StreamPosition = streamPosition;
                StreamName = @event.StreamName;
                EventData = @event.EventData;
                EventMetadata = @event.EventMetadata;
            }

            public string StreamName { get; set; }

            public IEventPersistence.Event ToEvent()
            {
                return new IEventPersistence.Event(StreamName,StreamPosition, EventData, EventMetadata);
            }
        }
    }
}