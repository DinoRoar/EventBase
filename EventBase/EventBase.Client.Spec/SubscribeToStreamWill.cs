using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EventBase.Client.Spec
{
    public class InMemoryEventStore_SubscribeToStreamWill
    {
        [Fact]
        public async void WhenSubscribedToStreamThenWillGetNewEvents()
        {
            var ut = new InMemoryEventStore();

            StreamEvent outputEvent = null;
            ut.SubscribeToStream("test-1", e =>
            {
                outputEvent = e;
            } );

            ut.Append(new CreateStreamEvent("test-1",StreamPositions.CreateNewStream, new Event()));

            var sla = TimeSpan.FromMilliseconds(100);
            var totalDelay = TimeSpan.Zero;
            while (outputEvent == null)
            {
                var delay = TimeSpan.FromMilliseconds(10);
                await Task.Delay(delay);
                totalDelay = totalDelay + delay;
                if (totalDelay > sla)
                {
                    throw new TimeoutException("Test did not pass within sla");
                }
            }
        }

        [Fact]
        public async void GivenEventsInStreamThenSubscriberWillGetTheseEvents()
        {
            var ut = new InMemoryEventStore();

            StreamEvent outputEvent = null;
            ut.Append(new CreateStreamEvent("test-1", StreamPositions.CreateNewStream, new Event()));

            ut.SubscribeToStream("test-1", e =>
            {
                outputEvent = e;
            });

            var sla = TimeSpan.FromMilliseconds(100);
            var totalDelay = TimeSpan.Zero;
            while (outputEvent == null)
            {
                var delay = TimeSpan.FromMilliseconds(10);
                await Task.Delay(delay);
                totalDelay = totalDelay + delay;
                if (totalDelay > sla)
                {
                    throw new TimeoutException("Test did not pass within sla");
                }
            }
        }


        [Fact]
        public async void WhenSubscribedToStreamThenWillGetNewEventsFromPosition()
        {
            var ut = new InMemoryEventStore();

            int totalEvents = 0;
            ut.Append(new CreateStreamEvent("test-1", StreamPositions.CreateNewStream, new Event()));
            ut.SubscribeToStream("test-1",1, e =>
            {
                totalEvents++;
            });

            ut.Append(new CreateStreamEvent("test-1", StreamPositions.Any, new Event()));

            var sla = TimeSpan.FromMilliseconds(100);
            var totalDelay = TimeSpan.Zero;
            while (totalEvents != 1)
            {
                var delay = TimeSpan.FromMilliseconds(10);
                await Task.Delay(delay);
                totalDelay = totalDelay + delay;
                if (totalDelay > sla)
                {
                    throw new TimeoutException("Test did not pass within sla");
                }
            }
        }

        [Fact]
        public async void WhenSubscribedToStreamThenWillGetAllEventsFromFirstposition()
        {
            var ut = new InMemoryEventStore();

            int totalEvents = 0;
            ut.Append(new CreateStreamEvent("test-1", StreamPositions.CreateNewStream, new Event()));
            ut.SubscribeToStream("test-1", 0, e =>
            {
                totalEvents++;
            });

            ut.Append(new CreateStreamEvent("test-1", StreamPositions.Any, new Event()));

            var sla = TimeSpan.FromMilliseconds(100);
            var totalDelay = TimeSpan.Zero;
            while (totalEvents != 2)
            {
                var delay = TimeSpan.FromMilliseconds(10);
                await Task.Delay(delay);
                totalDelay = totalDelay + delay;
                if (totalDelay > sla)
                {
                    throw new TimeoutException("Test did not pass within sla");
                }
            }
        }

        [Fact]
        public void SubscribingToStreamAtNonExistentPositionExplodes()
        {
            var ut = new InMemoryEventStore();
            int totalEvents = 0;
            ut.Append(new CreateStreamEvent("test-1", StreamPositions.CreateNewStream, new Event()));
            Assert.Throws<StreamToShortException>(() => ut.SubscribeToStream("test-1", 5, e =>
            {
                totalEvents++;
            }));

        }
    }
}
