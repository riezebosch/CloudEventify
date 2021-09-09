using System.Net.Mime;
using System.Threading.Tasks;
using Dapr.Client;
using FluentAssertions.Extensions;
using Hypothesist;
using MassTransit.Context;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Wrapr;
using Xunit;
using Xunit.Abstractions;

namespace MassTransit.CloudEvents.IntegrationTests
{
    public class FromDapr
    {
        private readonly ITestOutputHelper _output;

        public FromDapr(ITestOutputHelper output) => 
            _output = output;

        [Fact]
        public async Task Do()
        {
            var hypothesis = Hypothesis
                .For<UserLoggedIn>()
                .Any(x => x == new UserLoggedIn("1234"));

            using var logger = _output.BuildLogger();
            await Host(hypothesis, logger);
            await Publish(logger);

            await hypothesis.Validate(15.Seconds());
        }

        private static async Task Host(IHypothesis<UserLoggedIn> hypothesis, ILogger logger)
        {
            var consumer = Substitute.For<IConsumer<UserLoggedIn>>();
            consumer
                .When(x => x.Consume(Arg.Any<ConsumeContext<UserLoggedIn>>()))
                .Do(x => hypothesis.Test(x.Arg<ConsumeContext<UserLoggedIn>>().Message));

            LogContext.ConfigureCurrentLogContext(logger);
            var bus = Bus.Factory
                .CreateUsingRabbitMq(cfg =>
                {
                    cfg.UseCloudEventsFor(new ContentType("text/plain"), new ContentType("application/cloudevents+json"));
                    cfg.ReceiveEndpoint("user:loggedIn:test", e =>
                    {
                        e.Consumer(() => consumer);
                        e.Bind("user/loggedIn");
                    });

                    cfg.Message<UserLoggedIn>(x => x.SetEntityName("user/loggedIn"));
                });

            await bus.StartAsync();
        }

        private static async Task Publish(ILogger logger)
        {
            await using var sidecar = new Sidecar("from-dapr", logger);
            await sidecar.Start(with => with
                .ComponentsPath("components")
                .DaprGrpcPort(3001));

            using var client = new DaprClientBuilder()
                .UseGrpcEndpoint("http://localhost:3001")
                .Build();

            await client.PublishEventAsync("my-pubsub", "user/loggedIn", new { id = "1234" });
        }

        public record UserLoggedIn(string Id);
    }
}