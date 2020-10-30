using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EventBase.Core
{
    public class EventStore
    {
        private readonly IEventPersistence _persistenceLayer;

        public EventStore(IEventPersistence persistenceLayer)
        {
            _persistenceLayer = persistenceLayer;
        }
        public async Task<AppendResult> Append(string streamName, long streamPosition, byte[] eventData, byte[] eventMetadata)
        {
            var currentPosition = await _persistenceLayer.GetNextStreamPosition(streamName);

            async Task<AppendResult> JustWriteTheEvent()
            {
                var result = await _persistenceLayer.AppendEvent(new IEventPersistence.Event(streamName, currentPosition.Position, eventData, eventMetadata));
                return new AppendResult(result.NextPosition);
            }

            async Task<AppendResult> CreateNewStream()
            {
                if (!currentPosition.StreamDoesNotExist)
                {
                    throw new StreamAlreadyExistsException(streamName);
                }

                await _persistenceLayer.AppendEvent(new IEventPersistence.Event(streamName, currentPosition.Position, eventData, eventMetadata));
                return new AppendResult(1);
            }

            switch (streamPosition)
            {
                case StreamPositions.Any:
                    return await JustWriteTheEvent();
                case StreamPositions.Create:
                    {
                        return await CreateNewStream();
                    }
                default:
                    {
                        var nextStreamPosition = await _persistenceLayer.GetNextStreamPosition(streamName);
                        if (nextStreamPosition.StreamDoesNotExist) throw new StreamDoesNotExistException(streamName);
                        if (nextStreamPosition.Position != streamPosition) throw new UnexpectedStreamPositionException(streamPosition, nextStreamPosition.Position, streamName);

                        throw new UnexpectedStreamPositionException(streamPosition, -1, streamName);
                    }
            }
        }


        public async IAsyncEnumerable<StreamEvent> ReadStreamForward(string streamName, long fromPosition)
        {
            await foreach (var e in _persistenceLayer.ReadStreamForward(streamName, fromPosition))
            {
                yield return new StreamEvent(e.StreamName, e.EventData, e.EventMetadata, e.StreamPosition);
            }
        }

        public class StreamPositions
        {
            public const long Create = -2;
            public const long Any = -1;
            public const long NewStream = 0;
        }

        public class StreamEvent
        {
            public string StreamName { get; }
            public byte[] EventData { get; }
            public byte[] EventMetadata { get; }
            public long StreamPosition { get; }

            public StreamEvent(string streamName, byte[] eventData, byte[] eventMetadata, in long streamPosition)
            {
                StreamName = streamName;
                EventData = eventData;
                EventMetadata = eventMetadata;
                StreamPosition = streamPosition;
            }
        }
    }

    public class StreamAlreadyExistsException : Exception
    {
        public StreamAlreadyExistsException(string streamName) : base($"Attempted to create stream that already exists: {streamName}")
        {

        }
    }

    public class AppendResult
    {
        public AppendResult(long nextPosition)
        {
            NextPosition = nextPosition;
        }

        public long NextPosition { get; }
    }

    public class UnexpectedStreamPositionException : Exception
    {
        public UnexpectedStreamPositionException(long expectedPosition, long actualPosition, string streamName) : base(
            $"Failed to append event. Stream position {actualPosition} dit not match expected {expectedPosition} in stream {streamName}")
        {

        }
    }

    public class StreamDoesNotExistException : Exception
    {
        public StreamDoesNotExistException(string streamName) : base(
            $"Attempted to append event with position to a stream that does not exist: {streamName}")
        {

        }
    }

    public class FileBasedEventStore
    {
    }
}
