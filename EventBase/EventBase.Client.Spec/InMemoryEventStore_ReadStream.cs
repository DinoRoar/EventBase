using Xunit;

namespace EventBase.Client.Spec
{
    public class InMemoryEventStore_ReadStream
    {
        [Fact]
        public void GivenEmptyStream_ThenEmptyStream()
        {
            var ut = new InMemoryEventStore();
            var events = ut.ReadStream("test");
            Assert.Empty(events);
        }
    }
}