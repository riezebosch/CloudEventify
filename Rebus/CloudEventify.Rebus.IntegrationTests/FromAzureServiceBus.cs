using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using Bogus;
using FluentAssertions.Extensions;
using Hypothesist;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Messages;
using Rebus.Pipeline;
using Xunit;
using Xunit.Abstractions;

namespace CloudEventify.Rebus.IntegrationTests;

public class FromAzureServiceBus
{
    private const string ConnectionString = "sbmanuel.servicebus.windows.net";
    private const string Topic = "io.cloudevents.demo.user.loggedIn";
    private readonly ITestOutputHelper _output;

    public FromAzureServiceBus(ITestOutputHelper output) => 
        _output = output;

    /// <summary>
    /// Role assignment: Azure Service Bus Data Owner
    /// </summary>
    [Fact]
    public async Task Do()
    {
        // Arrange
        const string queue = "user:loggedIn:asb";

        var message = Message();
        var hypothesis = Hypothesis
            .For<(IMessageContext c, UserLoggedIn m)>()
            .Any(x => x.m == message)
            .Any(x => x.c.Headers[Headers.SenderAddress] == "cloudeventify:somewhere")
            .Any(x => x.c.Headers[Headers.CorrelationId] == "some-correlation-id");

        using var activator = new BuiltinHandlerActivator()
            .Handle<UserLoggedIn>(async (_, c, m) => await hypothesis.Test((c, m)));
        using var subscriber = Configure.With(activator)
            .Transport(t => t.UseAzureServiceBus($"Endpoint={ConnectionString}", queue, new DefaultAzureCredential()))
            .Options(o => o
                .UseCustomTypeNameForTopicName()
                .RemoveOutgoingRebusHeaders()
                .InjectMessageId())
            .Serialization(s => s.UseCloudEvents()
                .AddWithCustomName<UserLoggedIn>(Topic))
            .Logging(l => l.MicrosoftExtensionsLogging(_output.ToLoggerFactory()))
            .Start();
        await subscriber.Subscribe<UserLoggedIn>();

        // Act
        await Publish(ConnectionString, Topic, message);

        // Assert
        await hypothesis.Validate(5.Seconds());
    }

    /// <summary>
    /// Copied from: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/service-bus?view=azure-dotnet#code-example
    /// </summary>
    private static async Task Publish(string connectionString, string topic, UserLoggedIn message)
    {
        await using var client = new ServiceBusClient(connectionString, new DefaultAzureCredential());
        var sender = client.CreateSender(topic);

        var cloudEvent = new CloudEvent("cloudeventify:somewhere", topic, message);
        cloudEvent.ExtensionAttributes["traceparent"] = "some-correlation-id";
        
        await sender.SendMessageAsync(new ServiceBusMessage(new BinaryData(cloudEvent))
        {
            ContentType = "application/cloudevents+json",
            To = topic
        });
    }

    public record UserLoggedIn(string Id);
        
    private static UserLoggedIn Message() => 
        new Faker<UserLoggedIn>()
            .CustomInstantiator(f => new(f.Random.Hash()))
            .Generate();
}