using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventBase.Core
{
    public class FileBasedPersistenceLayer : IEventPersistence
    {
        public IAsyncEnumerable<IEventPersistence.Event> GetAllEvents()
        {
            throw new System.NotImplementedException();
        }

        public Task<IEventPersistence.AppendEventResult> AppendEvent(IEventPersistence.Event @event)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEventPersistence.GetStreamPositionResult> GetNextStreamPosition(string streamName)
        {
            throw new System.NotImplementedException();
        }

        public IAsyncEnumerable<IEventPersistence.Event> ReadStreamForward(string streamName, long fromPosition)
        {
            throw new System.NotImplementedException();
        }
    }
}