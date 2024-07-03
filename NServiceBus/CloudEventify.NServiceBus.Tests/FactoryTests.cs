using FluentAssertions.Extensions;
using Hypothesist;
using NServiceBus;

namespace CloudEventify.NServiceBus.Tests;

public class FactoryTests
{
    [Fact]
    public async Task SendPlain()
    {
        Handler.Hypothesis = Hypothesis.For<SerializerTests.A.UserLoggedIn>()
            .Any(x => x.Id == 1234);

        var endpointConfig = new EndpointConfiguration("CloudEventify.NServiceBus.Tests");
        endpointConfig.UseCloudEvents().WithTypes(t => t.Map<SerializerTests.A.UserLoggedIn>("UserLoggedIn"));
        endpointConfig.UseTransport<LearningTransport>();

        var endpointInstance = await Endpoint.Start(endpointConfig);
        await endpointInstance.SendLocal(new SerializerTests.A.UserLoggedIn(1234));
        await endpointInstance.Subscribe<SerializerTests.A.UserLoggedIn>();
        Thread.Sleep(1000);
    }

    public class Handler : IHandleMessages<SerializerTests.A.UserLoggedIn>
    {
        public static IHypothesis<SerializerTests.A.UserLoggedIn>? Hypothesis;

        public Task Handle(SerializerTests.A.UserLoggedIn message, IMessageHandlerContext context)
        {
            return Hypothesis?.Validate(1.Ticks(), context.CancellationToken) ??
                   Task.FromCanceled(context.CancellationToken);
        }
    }
}