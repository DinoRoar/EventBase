using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EventBase.Service.Spec
{
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
        }
    }

    public class DoesNothingTest : IClassFixture<TestWebApplicationFactory<EventBase.Service.Startup>>
    {
        private readonly TestWebApplicationFactory<Startup> _factory;

        public DoesNothingTest(TestWebApplicationFactory<EventBase.Service.Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public void Test1()
        {
            var client = _factory.CreateClient();
        }
    }
}
