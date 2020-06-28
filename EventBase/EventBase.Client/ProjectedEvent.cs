using System;

namespace EventBase.Client
{
    internal class ProjectedEvent : StreamEvent
    {
        public ProjectedEvent(string projectedStreamName, StreamEvent streamEvent)
            : base(projectedStreamName, StreamPositions.Any, StreamPositions.Any, DateTime.Now, streamEvent)
        {

        }

        public override StreamEvent SetStreamPositions(int streamPosition, in int globalPosition)
        {
            var e = new ProjectedEvent(StreamName, this)
            {
                StreamPosition = streamPosition, 
                GlobalPosition = globalPosition
            };
            return e;
        }
    }
}