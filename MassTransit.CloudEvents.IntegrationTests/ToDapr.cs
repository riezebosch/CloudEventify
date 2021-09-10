using System.Threading.Tasks;
using FluentAssertions.Extensions;
using Hypothesist;
using DaprApp;
using DaprApp.Controllers;
using MassTransit.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Wrapr;
using Xunit;
using Xunit.Abstractions;

namespace MassTransit.CloudEvents.IntegrationTests
{
    public class ToDapr
    {
        private readonly ITestOutputHelper _output;

        public ToDapr(ITestOutputHelper output) => 
            _output = output;

        [Fact]
        public async Task Do()
        {
            var hypothesis = Hypothesis
                .For<int>()
                .Any(x => x == 1234);

            using var logger = _output.BuildLogger();
            using var host = await Host(hypothesis);
            await using var sidecar = await Sidecar(logger);
            await Publish(logger);

            await hypothesis.Validate(10.Seconds());
        }

        private static async Task<Sidecar> Sidecar(ILogger logger)
        {
            var sidecar = new Sidecar("to-dapr", logger);
            await sidecar.Start(with => with
                .ComponentsPath("components")
                .AppPort(6000));

            return sidecar;
        }

        private async Task<Microsoft.Extensions.Hosting.IHost> Host(IHypothesis<int> hypothesis)
        {
            var handler = Substitute.For<IUserLoggedIn>();
            handler
                .When(x => x.Handle(Arg.Any<int>()))
                .Do(x => hypothesis.Test(x.Arg<int>()));

            var host = new HostBuilder().ConfigureWebHost(app => app
                    .UseStartup<Startup>()
                    .ConfigureLogging(builder => builder.AddXunit(_output))
                    .ConfigureServices(services => services.AddSingleton(handler))
                    .UseKestrel(options => options.ListenLocalhost(6000)))
                .Build();
            await host.StartAsync();
            return host;
        }

        private static async Task Publish(ILogger logger)
        {
            LogContext.ConfigureCurrentLogContext(logger);
            
            var bus = Bus.Factory
                .CreateUsingRabbitMq(cfg =>
                {
                    cfg.UseCloudEvents();
                    cfg.Message<UserLoggedIn>(x =>
                    {
                        x.SetEntityName("user/loggedIn");
                    });

                    cfg.PublishTopology.GetMessageTopology<UserLoggedIn>().Exclude = true;
                });

            await bus.StartAsync();
            
            var endpoint = await bus.GetPublishSendEndpoint<UserLoggedIn>();
            await endpoint.Send(new UserLoggedIn(1234));
        }

        private record UserLoggedIn(int UserId);
    }
}