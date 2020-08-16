using System;
using System.Collections.Generic;

namespace EventBase.Client
{
    public interface IEventStore
    {
        void Append(CreateStreamEvent createStreamEvent);

        void AddNewProjection(Projection projection);

        void SubscribeToStream(string streamName, Action<StreamEvent> onEvent);
        void SubscribeToStream(string streamName, int position, Action<object> onEvent);

        List<StreamEvent> ReadStream(string streamName);

        StreamPositions GetPosition(string eventStream);

        public class InvalidStreamPosition : InvalidOperationException
        {
            public InvalidStreamPosition(string streamName, in long expectedPosition, in long actualPosition)
                : base($"Attempted to append event to stream with invalid position: streamName: {streamName}, expectedPosition: {expectedPosition}, actualPosition: {actualPosition} ")
            {
                Data.Add("streamName", streamName);
                Data.Add("expectedPosition", expectedPosition);
                Data.Add("actualPosition", actualPosition);
            }
        }
    }
}