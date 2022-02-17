using System;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions.Extensions;
using Hypothesist;
using Hypothesist.Rebus;
using RabbitMQ.Client;
using Rebus.Activation;
using Rebus.Config;
using Wrapr;
using Xunit;
using Xunit.Abstractions;
using CloudEventity.Dapr;

namespace CloudEventify.Rebus.IntegrationTests;

[Collection("user/loggedIn")]
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
        const string topic = "user/loggedIn";
        const string queue = "user:loggedIn:rabbitmq";
        
        var message = Message();
        var hypothesis = Hypothesis
            .For<UserLoggedIn>()
            .Any(x => x == message);

        var activator = new BuiltinHandlerActivator()
            .Register(hypothesis.AsHandler);
        var subscriber = Configure.With(activator)
            .Transport(t => t.UseRabbitMq(_container.ConnectionString, queue))
            .Serialization(s => s.UseCloudEvents()
                .WithTypes(t => t.Map<UserLoggedIn>("loggedIn")))
            .Logging(l => l.MicrosoftExtensionsLogging(_output.ToLoggerFactory()))
            .Start();
        await subscriber.Subscribe<UserLoggedIn>();

        BindRebus(_container.ConnectionString, topic, queue);
        
        // Act
        await Publish(topic, message, _output);

        // Assert
        await hypothesis.Validate(5.Seconds());
    }
    
    private static void BindRebus(string connectionString, string topic, string queue)
    {
        var model = new ConnectionFactory
            {
                Endpoint = new AmqpTcpEndpoint(new Uri(connectionString))
            }
            .CreateConnection()
            .CreateModel();
        
        model.ExchangeDeclare(topic, "fanout", durable: true);
        model.QueueBind(queue, topic, "");
    }

    private static async Task Publish(string topic, UserLoggedIn message, ITestOutputHelper logger)
    {
        await using var sidecar = new Sidecar("from-dapr-to-rabbitmq", logger.ToLogger<Sidecar>());
        await sidecar.Start(with => with
            .ComponentsPath("components")
            .DaprGrpcPort(3201));

        using var client = CloudEventClientBuilder
            .For("http://localhost:3201")
            .WithTypes(types => types.Map<UserLoggedIn>("loggedIn"))
            .Build();
        
        await client
            .PublishEvent("my-pubsub", topic, message);
    }

    public record UserLoggedIn(string Id);
        
    private static UserLoggedIn Message() => 
        new Faker<UserLoggedIn>()
            .CustomInstantiator(f => new UserLoggedIn(f.Random.Hash()))
            .Generate();
}