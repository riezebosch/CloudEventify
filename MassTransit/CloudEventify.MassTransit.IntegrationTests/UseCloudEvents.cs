using System.Threading.Tasks;
using Bogus;
using FluentAssertions.Extensions;
using Hypothesist;
using MassTransit;
using Xunit;
using Xunit.Abstractions;

namespace CloudEventify.MassTransit.IntegrationTests;

[Collection("rabbitmq")]
public class UseCloudEvents : IClassFixture<RabbitMqContainer>
{
    private readonly ITestOutputHelper _output;
    private readonly RabbitMqContainer _container;

    public UseCloudEvents(ITestOutputHelper output, RabbitMqContainer container)
    {
        _output = output;
        _container = container;
    }
    
    [Fact]
    public async Task Do()
    {
        // Arrange
        var message = new Faker<Request>().CustomInstantiator(f => new Request(f.Random.Number())).Generate();
        var observer = Observer
            .For<Reply>();

        LogContext.ConfigureCurrentLogContext(_output.ToLoggerFactory());
        var bus = Bus.Factory
            .CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(_container.ConnectionString);
                cfg.UseCloudEvents()
                    .WithTypes(t => t
                        .Map<Request>("request")
                        .Map<Reply>("reply"));
                
                cfg.ReceiveEndpoint("a", e => 
                    e.Handler<Request>(m => m.Publish(new Reply(m.Message.UserId))));
                    
                cfg.ReceiveEndpoint("user:loggedIn:test", e =>
                {
                    e.Handler<Reply>(x => observer.Add(x.Message));
                    e.Bind("user/loggedIn");
                });
            });


        await bus.StartAsync();

        // Act
        var endpoint = await bus.GetPublishSendEndpoint<Request>();
        await endpoint.Send(message);

        // Assert
        await Hypothesis
            .On(observer)
            .Timebox(30.Seconds())
            .Any()
            .Match(x => x.Id == message.UserId)
            .Validate();
    }
    
    public record Request(int UserId);
    public record Reply(int Id);
}

