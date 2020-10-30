using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventBase.Core
{
    public interface IEventPersistence
    {
        IAsyncEnumerable<Event> GetAllEvents();
        Task<AppendEventResult> AppendEvent(Event @event);
        Task<GetStreamPositionResult> GetNextStreamPosition(string streamName);
        IAsyncEnumerable<Event> ReadStreamForward(string streamName, long fromPosition);

        public class Event
        {
            public string StreamName { get; }
            public byte[] EventData { get; }
            public byte[] EventMetadata { get; }
            public long StreamPosition { get; }

            public Event(string streamName, long streamPosition, byte[] eventData, byte[] eventMetadata)
            {
                StreamName = streamName;
                StreamPosition = streamPosition;
                EventData = eventData;
                EventMetadata = eventMetadata;
            }
        }

        public class AppendEventResult
        {
            public AppendEventResult(long nextPosition)
            {
                NextPosition = nextPosition;
            }

            public long NextPosition { get; }
        }
        
        public class GetStreamPositionResult
        {
            public GetStreamPositionResult()
            {
            }

            public GetStreamPositionResult(long position)
            {
                if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
                Position = position;
            }

            public bool StreamDoesNotExist => Position == 0;
            public long Position { get; }
        }
    }
}
