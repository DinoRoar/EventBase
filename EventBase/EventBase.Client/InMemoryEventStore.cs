using System;
using System.Collections.Generic;
using System.Linq;

namespace EventBase.Client
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly Dictionary<string, List<StreamEvent>> _streams = new Dictionary<string, List<StreamEvent>>();
        private readonly List<StreamEvent> _allEvents = new List<StreamEvent>();
        private readonly List<Projection> _projections = new List<Projection>();
        private readonly Dictionary<string, List<Action<StreamEvent>>> _subscriptions = new Dictionary<string, List<Action<StreamEvent>>>();

        public InMemoryEventStore()
        {
            BuildDefaultProjections();
        }

        private void BuildDefaultProjections()
        {
            _projections.Add(new CategoryProjection());
            _projections.Add(new EventTypeProjection());
        }

        public void Append(StreamEvent streamEvent)
        {
            StreamEvent @event;
            var nextGlobalEventVersion = _allEvents.Count;
            if (!_streams.ContainsKey(streamEvent.StreamName))
            {
                CheckStreamPosition(streamEvent, 0);
                @event = streamEvent.SetStreamPositions(0, nextGlobalEventVersion);
                _streams.Add(@event.StreamName, new List<StreamEvent>() { @event});
            }
            else
            {
                var stream = _streams[streamEvent.StreamName];
                var nextPosition = stream.Count;

                if (streamEvent.StreamPosition != StreamPositions.Any)
                {
                    CheckStreamPosition(streamEvent, nextPosition);
                }

                @event = streamEvent.SetStreamPositions(nextPosition, nextGlobalEventVersion);
                stream.Add(@event);
            }

            _allEvents.Add(@event);

            ExecuteSubscriptions(@event);
            RunProjections(@event);
        }

        private void ExecuteSubscriptions(StreamEvent @event)
        {
            if (_subscriptions.ContainsKey(@event.StreamName))
            {
                _subscriptions[@event.StreamName].ForEach(s => s(@event));
            }
        }

        private void RunProjections(StreamEvent streamEvent)
        {
            foreach (var projection in _projections)
            {
                if (projection.Predicate(streamEvent))
                {
                    Append(new ProjectedEvent(projection.StreamName(streamEvent), streamEvent));
                }
            }
        }

        public void AddNewProjection(Projection projection)
        {
            _projections.Add(projection);
        }

        public void SubscribeToStream(string streamName, Action<StreamEvent> onEvent)
        {
            if (!_subscriptions.ContainsKey(streamName))
            {
                _subscriptions.Add(streamName, new List<Action<StreamEvent>>());
            }

            _subscriptions[streamName].Add(onEvent);
        }

        public List<StreamEvent> ReadStream(string streamName)
        {
            if (!_streams.ContainsKey(streamName))
            {
                return new List<StreamEvent>();
            }
            return _streams[streamName];
        }

        public StreamPositions GetPosition(string eventStream)
        {
            if (!_streams.ContainsKey(eventStream))
            {
                return new StreamPositions(0, _allEvents.Count);
            }
            var streamEvents = _streams[eventStream];
            if (!streamEvents.Any())
            {
                return new StreamPositions(0, _allEvents.Count);
            }
            var lastEvent = streamEvents.Last();
            return new StreamPositions(lastEvent.StreamPosition + 1, lastEvent.GlobalPosition + 1);
        }

        private static void CheckStreamPosition(StreamEvent streamEvent, long currentPosition)
        {
            switch (streamEvent.StreamPosition)
            {
                case StreamPositions.CreateNewStream:
                {
                    if (currentPosition != 0)
                    {
                        throw new IEventStore.InvalidStreamPosition(
                            streamEvent.StreamName,
                            streamEvent.StreamPosition,
                            currentPosition);
                    }

                    break;
                }
                case StreamPositions.Any:
                {
                    break;
                }
                default:
                {
                    if (streamEvent.StreamPosition != currentPosition)
                    {
                        throw new IEventStore.InvalidStreamPosition(
                            streamEvent.StreamName,
                            streamEvent.StreamPosition,
                            currentPosition);
                    }

                    break;
                }
            }
        }
    }
}