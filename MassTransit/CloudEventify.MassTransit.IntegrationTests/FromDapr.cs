using System.Net.Mime;
using System.Threading.Tasks;
using Bogus;
using Dapr.Client;
using FluentAssertions.Extensions;
using Hypothesist;
using MassTransit;
using MassTransit.Context;
using Wrapr;
using Xunit;
using Xunit.Abstractions;

namespace CloudEventify.MassTransit.IntegrationTests;

[Collection("rabbitmq")]
public class FromDapr : IClassFixture<RabbitMqContainer>
{
    private readonly ITestOutputHelper _output;
    private readonly RabbitMqContainer _container;

    public FromDapr(ITestOutputHelper output, RabbitMqContainer container)
    {
        _output = output;
        _container = container;
    }

    [Fact]
    public async Task Do()
    {
        // Arrange
        var message = Message();
        var hypothesis = Hypothesis
            .For<UserLoggedIn>()
            .Any(x => x == message);

        LogContext.ConfigureCurrentLogContext(_output.ToLoggerFactory());
            
        var bus = Bus.Factory
            .CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(_container.ConnectionString);
                cfg.UseCloudEvents()
                    .WithContentType(new ContentType("text/plain"));
                    
                cfg.ReceiveEndpoint("user:loggedIn:test", e =>
                {
                    e.Consumer(hypothesis.AsConsumer);
                    e.Bind("user/loggedIn");
                });
                    
                // cfg.Message<UserLoggedIn>(x => x.SetEntityName("user/loggedIn"));
            });

        await bus.StartAsync();
            
        // Act
        await Publish(message, _output);

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

        LogContext.ConfigureCurrentLogContext(_output.ToLoggerFactory());
            
        var bus = Bus.Factory
            .CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(_container.ConnectionString);
                cfg.ReceiveEndpoint("user:loggedIn:test:local-config", x =>
                {
                    x.UseCloudEvents()
                        .WithContentType(new ContentType("text/plain"));
                    x.Consumer(hypothesis.AsConsumer);
                    x.Bind("user/loggedIn");
                });
                
                // cfg.Message<UserLoggedIn>(x => x.SetEntityName("user/loggedIn"));
            });
        await bus.StartAsync();

        // Act
        await Publish(message, _output);

        // Assert
        await hypothesis.Validate(10.Seconds());
    }


    private static async Task Publish(UserLoggedIn message, ITestOutputHelper logger)
    {
        await using var sidecar = new Sidecar("from-dapr", logger.ToLogger<Sidecar>());
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