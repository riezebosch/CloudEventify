using System.Threading.Tasks;
using CloudEventify.Rebus;
using FluentAssertions.Extensions;
using Hypothesist;
using Hypothesist.Rebus;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using Xunit;

namespace CloudEventify.Rebus.Tests;

public class SerializerTests
{
    [Fact]
    public async Task Send()
    {
        var hypothesis = Hypothesis.For<A.UserLoggedIn>()
            .Any(x => x.Id == 1234);

        using var activator = new BuiltinHandlerActivator();
        activator.Register(hypothesis.AsHandler);
        
        var bus = Configure
            .With(activator)
            .UseCloudEvents(c => c.WithTypes(t => t.Map<A.UserLoggedIn>("user.loggedIn", m => m with { Subject = x => $"user/{x.Id}" })).WithSource(new System.Uri("uri:MySourceApp")))
            .Transport(s => s.UseInMemoryTransport(new InMemNetwork(), "user"))
            .Subscriptions(s => s.StoreInMemory())
            .Routing(r => r.TypeBased().Map<A.UserLoggedIn>("user"))
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
            .UseCloudEvents(c => c.WithTypes(t => t.Map<int>("int")
                    .Map<B.UserLoggedIn>("user.loggedIn")))
            .Transport(s => s.UseInMemoryTransport(network, "producer"))
            .Subscriptions(s => s.StoreInMemory())
            .Routing(r => r.TypeBased().Map<int>("consumer"))
            .Start();

    private static IBus Consumer(IHandlerActivator activator, InMemNetwork network) =>
        Configure
            .With(activator)
            .UseCloudEvents(c => c.WithTypes(t => t.Map<int>("int")
                    .Map<A.UserLoggedIn>("user.loggedIn")))
            .Transport(s => s.UseInMemoryTransport(network, "consumer"))
            .Subscriptions(s => s.StoreInMemory())
            .Routing(r => r.TypeBased().Map<int>("consumer"))
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
