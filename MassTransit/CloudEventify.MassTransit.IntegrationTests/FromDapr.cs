using System.Threading.Tasks;
using Bogus;
using CloudEventify.Dapr;
using Dapr.Client;
using FluentAssertions.Extensions;
using Hypothesist;
using MassTransit;
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
                    .WithTypes(t => t.Map<UserLoggedIn>("user.loggedIn"));
                    
                cfg.ReceiveEndpoint("user:loggedIn:test", e =>
                {
                    e.Handler<UserLoggedIn>(x => hypothesis.Test(x.Message));
                    e.Bind("user/loggedIn");
                });
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
                        .WithTypes(types => types.Map<UserLoggedIn>("user.loggedIn"));
                    x.Handler<UserLoggedIn>(m => hypothesis.Test(m.Message));
                    x.Bind("user/loggedIn");
                });
            });
        await bus.StartAsync();

        // Act
        await Publish(message, _output);

        // Assert
        await hypothesis.Validate(30.Seconds());
    }


    private static async Task Publish(UserLoggedIn message, ITestOutputHelper logger)
    {
        await using var sidecar = new Sidecar("from-dapr", logger.ToLogger<Sidecar>());
        await sidecar.Start(with => with
            .ComponentsPath("components")
            .DaprGrpcPort(3001));

        using var client = new DaprClientBuilder()
            .UseGrpcEndpoint("http://localhost:3001")
            .UseCloudEvents()
            .WithTypes(types => types.Map<UserLoggedIn>("user.loggedIn"))
            .Build(); 
        
        await client.PublishEventAsync("my-pubsub", "user/loggedIn", message);
    }

    public record UserLoggedIn(string Id);
        
    private static UserLoggedIn Message() => 
        new Faker<UserLoggedIn>()
            .CustomInstantiator(f => new UserLoggedIn(f.Random.Hash()))
            .Generate();
}