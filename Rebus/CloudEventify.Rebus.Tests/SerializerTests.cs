using System.Threading.Tasks;
using FluentAssertions.Extensions;
using Hypothesist;
using Hypothesist.Rebus;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Messages.Control;
using Rebus.Persistence.InMem;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using Xunit;
using Xunit.Abstractions;

namespace CloudEventify.Rebus.Tests;

public class SerializerTests
{
    private readonly ITestOutputHelper _output;

    public SerializerTests(ITestOutputHelper output) => 
        _output = output;

    [Fact]
    public async Task Publish()
    {
        var hypothesis = Hypothesis.For<A.UserLoggedIn>()
            .Any(x => x.Id == 1234);
        
        var network = new InMemNetwork(true);
        var subscribers = new InMemorySubscriberStore();

        var builder = WebApplication.CreateBuilder();
        builder.Services
            .AddSingleton(hypothesis.AsHandler())
            .AddRebus(configure => configure
            .Transport(s => s.UseInMemoryTransport(network, "a"))
            .Subscriptions(s => s.StoreInMemory(subscribers))
            .Routing(r => r.TypeBased()
                .Map<A.UserLoggedIn>("b"))
            .UseCloudEvents(options => options.RegisterTypeWithCustomName<A.UserLoggedIn>("user.loggedIn")
                                              .RegisterTypeWithShortName<SubscribeRequest>())
            .Logging(l => l.MicrosoftExtensionsLogging(_output.ToLoggerFactory())),
                onCreated: async bus => await bus.Subscribe<A.UserLoggedIn>());

        await using var app = builder.Build();
        await app.StartAsync();
            
        var producer = Configure
            .With(new BuiltinHandlerActivator())
            .Transport(s => s.UseInMemoryTransport(network, "c"))
            .Subscriptions(s => s.StoreInMemory(subscribers))
            .UseCloudEvents(options => options.RegisterTypeWithCustomName<A.UserLoggedIn>("user.loggedIn"))
            .Logging(l => l.MicrosoftExtensionsLogging(_output.ToLoggerFactory()))
            .Start();

        await producer.Publish(new A.UserLoggedIn(1234));
        
        await hypothesis.Validate(2.Seconds());
    }
    
    [Fact]
    public async Task Send()
    {
        var hypothesis = Hypothesis.For<A.UserLoggedIn>()
            .Any(x => x.Id == 1234);

        using var activator = new BuiltinHandlerActivator();
        activator.Register(hypothesis.AsHandler);
        
        var bus = Configure
            .With(activator)
            .Transport(s => s.UseInMemoryTransport(new InMemNetwork(), "user"))
            .Subscriptions(s => s.StoreInMemory())
            .Routing(r => r.TypeBased().Map<A.UserLoggedIn>("user"))
            .UseCloudEvents(options => options.RegisterTypeWithCustomName<A.UserLoggedIn>("user.loggedIn")
                                              .RegisterTypeWithShortName<SubscribeRequest>())
            .Start();

        await bus.Subscribe<A.UserLoggedIn>();
        await bus.Send(new A.UserLoggedIn(1234));
        
        await hypothesis.Validate(2.Seconds());
    }
    
    [Fact]
    public async Task Reply()
    {
        // Arrange
        var network = new InMemNetwork();

        using var activator1 = new BuiltinHandlerActivator()
            .Handle<int>((b, m) => b.Reply(new A.UserLoggedIn(m)));
        using var consumer =  Consumer(activator1, network);
        await consumer.Subscribe<int>();
        
        var hypothesis = Hypothesis.For<B.UserLoggedIn>()
            .Any(x => x.Id == 1234);
        using var activator2 = new BuiltinHandlerActivator()
            .Register(hypothesis.AsHandler);
        using var producer = Producer(activator2, network);

        // Act
        await producer.Send(1234);
        
        // Assert
        await hypothesis.Validate(2.Seconds());
    }

    private static IBus Producer(IHandlerActivator activator, InMemNetwork network) =>
        Configure
            .With(activator)
            .Transport(s => s.UseInMemoryTransport(network, "producer"))
            .Subscriptions(s => s.StoreInMemory())
            .Routing(r => r.TypeBased().Map<int>("consumer"))
            .UseCloudEvents(options => options.RegisterTypeWithCustomName<B.UserLoggedIn>("user.loggedIn")
                                              .RegisterTypeWithCustomName<int>("int"))
            .Start();

    private static IBus Consumer(IHandlerActivator activator, InMemNetwork network) =>
        Configure
            .With(activator)
            .Transport(s => s.UseInMemoryTransport(network, "consumer"))
            .Subscriptions(s => s.StoreInMemory())
            .Routing(r => r.TypeBased().Map<int>("consumer"))
            .UseCloudEvents(options => options.RegisterTypeWithShortName<SubscribeRequest>()
                                              .RegisterTypeWithCustomName<int>("int")
                                              .RegisterTypeWithCustomName<A.UserLoggedIn>("user.loggedIn"))
            .Start();

    private static class A
    {
        public record UserLoggedIn(int Id);
    }

    private static class B
    {
        public record UserLoggedIn(int Id);
    }
}
