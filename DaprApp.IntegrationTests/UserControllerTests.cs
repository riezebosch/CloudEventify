using System.Threading.Tasks;
using Dapr.Client;
using FluentAssertions.Extensions;
using Hypothesist;
using DaprApp.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wrapr;
using Xunit;
using Xunit.Abstractions;

namespace DaprApp.IntegrationTests
{
    public class UserControllerTests
    {
        private readonly ITestOutputHelper _output;

        public UserControllerTests(ITestOutputHelper output) => 
            _output = output;

        [Fact]
        public async Task TestPublish()
        {
            var hypothesis = Hypothesis
                .For<int>()
                .Any(x => x == 1234);

            using var logger = _output.BuildLogger();
            using var host = await Host(hypothesis);
            await using var sidecar = await Sidecar(logger);
            await Publish();

            await hypothesis.Validate(10.Seconds());
        }

        private static async Task<Sidecar> Sidecar(ILogger logger)
        {
            var sidecar = new Sidecar("test-dapr", logger);
            await sidecar.Start(with => with
                .ComponentsPath("components")
                .DaprGrpcPort(3002)
                .AppPort(6002));

            return sidecar;
        }

        private async Task<IHost> Host(IHypothesis<int> hypothesis)
        {
            var host = new HostBuilder().ConfigureWebHost(app => app
                    .UseStartup<Startup>()
                    .ConfigureLogging(builder => builder.AddXunit(_output))
                    .ConfigureServices(services => services.AddSingleton<IHandler<int>>(new TestHandler<int>(hypothesis)))
                    .UseKestrel(options => options.ListenLocalhost(6002)))
                .Build();
            await host.StartAsync();
            return host;
        }

        private class TestHandler<T> : IHandler<T>
        {
            private readonly IHypothesis<T> _hypothesis;

            public TestHandler(IHypothesis<T> hypothesis) => 
                _hypothesis = hypothesis;

            public Task Handle(T id) =>
                _hypothesis.Test(id);
        }

        private static async Task Publish()
        {
            using var client = new DaprClientBuilder()
                .UseGrpcEndpoint("http://localhost:3002")
                .Build();

            await client
                .PublishEventAsync("my-pubsub", "user/loggedIn", new { UserId = 1234 });
        }
    }
}