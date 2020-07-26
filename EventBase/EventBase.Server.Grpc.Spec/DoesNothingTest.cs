using Xunit;

namespace EventBase.Server.Grpc.Spec
{
    public class DoesNothingTest : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private readonly TestWebApplicationFactory<Startup> _factory;

        public DoesNothingTest(TestWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public void Test1()
        {
          //  var client = _factory.CreateClient();
        }
    }
}