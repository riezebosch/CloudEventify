using System.Threading.Tasks;
using CloudEventify.Rebus;
using FluentAssertions.Extensions;
using Hypothesist;
using Hypothesist.Rebus;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using Xunit;

namespace CloudEventity.Rebus.Tests;

public class SerializerTests
{
    [Fact]
    public async Task Send()
    {
        var hypothesis = Hypothesis.For<UserLoggedIn>()
            .Any(x => x.Id == 1234);

        using var activator = new BuiltinHandlerActivator();
        activator.Register(hypothesis.AsHandler);
        
        var bus = Configure
            .With(activator)
            .Transport(s => s.UseInMemoryTransport(new InMemNetwork(false), "user"))
            .Subscriptions(s => s.StoreInMemory())
            .Routing(r => r.TypeBased().Map<UserLoggedIn>("user"))
            .Serialization(s => s.UseCloudEvents())
            .Start();

        await bus.Subscribe<UserLoggedIn>();
        await bus.Send(new UserLoggedIn(1234));
        
        await hypothesis.Validate(2.Seconds());
    }
    
    [Fact]
    public async Task Reply()
    {
        var hypothesis = Hypothesis.For<UserLoggedIn>()
            .Any(x => x.Id == 1234);

        using var consumer = new BuiltinHandlerActivator()
            .Handle<int>((b, m) => b.Reply(new UserLoggedIn(m)));

        var network = new InMemNetwork(false);
        var a = Configure
            .With(consumer)
            .Transport(s => s.UseInMemoryTransport(network, "consumer"))
            .Subscriptions(s => s.StoreInMemory())
            .Routing(r => r.TypeBased().Map<int>("consumer"))
            .Serialization(s => s.UseCloudEvents())
            .Start();

        await a.Subscribe<int>();
        
        using var producer = new BuiltinHandlerActivator()
            .Register(hypothesis.AsHandler);

        var b = Configure
            .With(producer)
            .Transport(s => s.UseInMemoryTransport(network, "producer"))
            .Subscriptions(s => s.StoreInMemory())
            .Routing(r => r.TypeBased().Map<int>("consumer"))
            .Serialization(s => s.UseCloudEvents())
            .Start();

        await b.Send(1234);
        
        await hypothesis.Validate(2.Seconds());
    }
}

public record UserLoggedIn(int Id);