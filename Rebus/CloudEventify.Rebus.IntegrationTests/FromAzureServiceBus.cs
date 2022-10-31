using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using Bogus;
using FluentAssertions.Extensions;
using Hypothesist;
using Hypothesist.Rebus;
using Rebus.Activation;
using Rebus.Config;
using Xunit;
using Xunit.Abstractions;

namespace CloudEventify.Rebus.IntegrationTests;

public class FromAzureServiceBus
{
    private const string ConnectionString = "sbmanuel.servicebus.windows.net";
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
            .For<UserLoggedIn>()
            .Any(x => x == message);

        using var activator = new BuiltinHandlerActivator()
            .Register(hypothesis.AsHandler);
        using var subscriber = Configure.With(activator)
            .Transport(t => t.UseAzureServiceBus($"Endpoint={ConnectionString}", queue, new DefaultAzureCredential()))
            .InjectMessageId()
            .UseCustomTypeNameForTopicName()
            .Serialization(s => s.UseCloudEvents()
                .AddWithCustomName<UserLoggedIn>("io.cloudevents.demo.user.loggedIn"))
            .Logging(l => l.MicrosoftExtensionsLogging(_output.ToLoggerFactory()))
            .Start();
        await subscriber.Subscribe<UserLoggedIn>();

        // Act
        await Publish(ConnectionString, "io/cloudevents/demo/user/loggedIn", message);

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

        await sender.SendMessageAsync(new ServiceBusMessage(new BinaryData(new CloudEvent("cloudeventify", "io.cloudevents.demo.user.loggedIn", message)))
        {
            ContentType = "application/cloudevents+json"
        });
    }

    public record UserLoggedIn(string Id);
        
    private static UserLoggedIn Message() => 
        new Faker<UserLoggedIn>()
            .CustomInstantiator(f => new(f.Random.Hash()))
            .Generate();
}