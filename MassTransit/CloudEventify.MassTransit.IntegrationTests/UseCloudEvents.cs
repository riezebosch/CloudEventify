using System.Net.Mime;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions.Extensions;
using Hypothesist;
using MassTransit;
using MassTransit.Context;
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
        var hypothesis = Hypothesis
            .For<Reply>()
            .Any(x => x.Id == message.UserId);

        LogContext.ConfigureCurrentLogContext(_output.ToLoggerFactory());
        var bus = Bus.Factory
            .CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(_container.ConnectionString);
                cfg.UseCloudEvents()
                    .WithContentType(new ContentType("text/plain"))
                    .WithTypes(t => t.Map<Request>("request").Map<Reply>("reply"));
                
                cfg.ReceiveEndpoint("a", e => e.Handler<Request>(m => m.Publish(new Reply(m.Message.UserId))));
                    
                cfg.ReceiveEndpoint("user:loggedIn:test", e =>
                {
                    e.Consumer(hypothesis.AsConsumer);
                    e.Bind("user/loggedIn");
                });
            });


        await bus.StartAsync();

        // Act
        var endpoint = await bus.GetPublishSendEndpoint<Request>();
        await endpoint.Send(message);

        // Assert
        await hypothesis.Validate(10.Seconds());
    }
    
    public record Request(int UserId);
    public record Reply(int Id);
}

