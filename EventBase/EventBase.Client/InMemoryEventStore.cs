using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EventBase.Client
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly Dictionary<string, List<StreamEvent>> _streams = new Dictionary<string, List<StreamEvent>>();
        private readonly List<StreamEvent> _allEvents = new List<StreamEvent>();
        private readonly List<Projection> _projections = new List<Projection>();
        private readonly Dictionary<string, List<Action<StreamEvent>>> _subscriptions = new Dictionary<string, List<Action<StreamEvent>>>();

        private readonly Mutex _writeMutex = new Mutex();

        public InMemoryEventStore()
        {
            BuildDefaultProjections();
        }

        private void BuildDefaultProjections()
        {
            AddNewProjection(new CategoryProjection());
            AddNewProjection(new EventTypeProjection());
        }

        public void Append(CreateStreamEvent createStreamEvent)
        {
            _writeMutex.WaitOne(TimeSpan.FromSeconds(1));
            try
            {
                var nextGlobalEventVersion = _allEvents.Count;
                var streamPosition = GetStreamPosition(createStreamEvent);

                if (streamPosition == StreamPositions.DoesNotExist)
                {
                    _streams.Add(createStreamEvent.StreamName, new List<StreamEvent>());
                    streamPosition = 0;
                }

                if (createStreamEvent.StreamPosition != StreamPositions.Any)
                {
                    CheckStreamPosition(createStreamEvent, streamPosition);
                }

                var @event = new StreamEvent(createStreamEvent, streamPosition, nextGlobalEventVersion);
                _streams[createStreamEvent.StreamName].Add(@event);
                _allEvents.Add(@event);

                ExecuteSubscriptions(@event);
                RunProjections(@event);
            }
            finally
            {
                _writeMutex.ReleaseMutex();
            }
        }

        private void Append(ProjectedEvent projectedEvent)
        {
            _writeMutex.WaitOne(TimeSpan.FromSeconds(1));
            try
            {
                var nextGlobalEventVersion = _allEvents.Count;
                var streamPosition = GetStreamPosition(projectedEvent);
                if (streamPosition == StreamPositions.DoesNotExist)
                {
                    _streams.Add(projectedEvent.StreamName, new List<StreamEvent>());
                    streamPosition = 0;
                }
                var @event = projectedEvent.SetPositions(streamPosition, nextGlobalEventVersion);
                _streams[projectedEvent.StreamName].Add(@event);
                _allEvents.Add(@event);

                ExecuteSubscriptions(@event);
                RunProjections(@event);
            }
            finally
            {
                _writeMutex.ReleaseMutex();
            }

        }

        private long GetStreamPosition(IHaveStreamName stream)
        {

            if (!_streams.ContainsKey(stream.StreamName))
            {
                return StreamPositions.DoesNotExist;
            }

            return _streams[stream.StreamName].Count;

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

            if (_streams.ContainsKey(streamName))
            {
                _streams[streamName].ForEach(onEvent);
            }
            
            _subscriptions[streamName].Add(onEvent);
        }

        public void SubscribeToStream(string streamName, int position, Action<object> onEvent)
        {
            if (!_subscriptions.ContainsKey(streamName))
            {
                _subscriptions.Add(streamName, new List<Action<StreamEvent>>());
            }

            if (_streams.ContainsKey(streamName))
            {
                var stream = _streams[streamName];
                if (stream.Count < position)
                {
                    throw new StreamToShortException(streamName);
                }
                var streamEvents = stream.Skip(position);
                foreach (var streamEvent in streamEvents)
                {
                    onEvent(streamEvent);
                }
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

        private static void CheckStreamPosition(CreateStreamEvent streamEvent, long currentPosition)
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

    public class StreamToShortException : InvalidOperationException
    {
        public StreamToShortException(string streamName)
        :base($"Invalid operation: {streamName} doesn't have enough events")
        {
            
        }
    }
}