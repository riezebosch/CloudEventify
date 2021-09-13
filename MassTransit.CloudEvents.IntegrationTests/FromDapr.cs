using System.Net.Mime;
using System.Threading.Tasks;
using Bogus;
using Dapr.Client;
using FluentAssertions.Extensions;
using Hypothesist;
using MassTransit.Context;
using Microsoft.Extensions.Logging;
using Wrapr;
using Xunit;
using Xunit.Abstractions;

namespace MassTransit.CloudEvents.IntegrationTests
{
    [Collection("user/loggedIn")]
    public class FromDapr
    {
        private readonly ITestOutputHelper _output;

        public FromDapr(ITestOutputHelper output) => 
            _output = output;

        [Fact]
        public async Task Do()
        {
            // Arrange
            var message = Message();
            var hypothesis = Hypothesis
                .For<UserLoggedIn>()
                .Any(x => x == message);

            using var logger = _output.BuildLogger();
            LogContext.ConfigureCurrentLogContext(logger);
            
            var bus = Bus.Factory
                .CreateUsingRabbitMq(cfg =>
                {
                    cfg.UseCloudEvents()
                        .WithContentType(new ContentType("text/plain"));
                    
                    cfg.ReceiveEndpoint("user:loggedIn:test", e =>
                    {
                        e.Consumer(hypothesis.ToConsumer);
                        e.Bind("user/loggedIn");
                    });
                    
                    cfg.Message<UserLoggedIn>(x => x.SetEntityName("user/loggedIn"));
                });

            await bus.StartAsync();
            
            // Act
            await Publish(message, logger);

            // Assert
            await hypothesis.Validate(15.Seconds());
        }

        [Fact]
        public async Task ReceiveEndpointConfiguration()
        {
            // Arrange
            var message = Message();
            var hypothesis = Hypothesis
                .For<UserLoggedIn>()
                .Any(x => x == message);

            using var logger = _output.BuildLogger();
            LogContext.ConfigureCurrentLogContext(logger);
            
            var bus = Bus.Factory
                .CreateUsingRabbitMq(cfg =>
                {
                    cfg.ReceiveEndpoint("user:loggedIn:test:local-config", x =>
                    {
                        x.UseCloudEvents()
                            .WithContentType(new ContentType("text/plain"));
                        x.Consumer(hypothesis.ToConsumer);
                        x.Bind("user/loggedIn");
                    });
                });
            await bus.StartAsync();

            // Act
            await Publish(message, logger);

            // Assert
            await hypothesis.Validate(10.Seconds());
        }


        private static async Task Publish(UserLoggedIn message, ILogger logger)
        {
            await using var sidecar = new Sidecar("from-dapr", logger);
            await sidecar.Start(with => with
                .ComponentsPath("components")
                .DaprGrpcPort(3001));

            using var client = new DaprClientBuilder()
                .UseGrpcEndpoint("http://localhost:3001")
                .Build();

            await client.PublishEventAsync("my-pubsub", "user/loggedIn", message);
        }

        public record UserLoggedIn(string Id);
        
        private static UserLoggedIn Message() => 
            new Faker<UserLoggedIn>()
                .CustomInstantiator(f => new UserLoggedIn(f.Random.Hash()))
                .Generate();
    }
}