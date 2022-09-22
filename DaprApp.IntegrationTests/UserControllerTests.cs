using System;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using FluentAssertions.Extensions;
using Hypothesist;
using DaprApp.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wrapr;
using Xunit;
using Xunit.Abstractions;

namespace DaprApp.IntegrationTests;

public class UserControllerTests
{
    private readonly ITestOutputHelper _output;

    public UserControllerTests(ITestOutputHelper output) => 
        _output = output;

    [Fact]
    public async Task TestPublish()
    {
        var hypothesis = Hypothesis
            .For<int>()
            .Any(x => x == 1234);

        await using var host = await Host(hypothesis);
        await using var sidecar = await Sidecar(_output);
        await Publish();

        await hypothesis.Validate(10.Seconds());
    }

    private static async Task<Sidecar> Sidecar(ITestOutputHelper logger)
    {
        var sidecar = new Sidecar("test-dapr", logger.ToLogger<Sidecar>());
        await sidecar.Start(with => with
            .ComponentsPath("components")
            .DaprGrpcPort(3002)
            .AppPort(6002));

        return sidecar;
    }

    private async Task<IAsyncDisposable> Host(IHypothesis<int> hypothesis)
    {
        var app = Startup.App(builder =>
        {
            builder
                .Services
                .AddSingleton<IHandler<int>>(new TestHandler<int>(hypothesis));
            builder
                .Logging
                .AddXUnit(_output);
        });

        app.Urls.Add("http://localhost:6002");
        await app.StartAsync();

        return app;
    }

    private class TestHandler<T> : IHandler<T>
    {
        private readonly IHypothesis<T> _hypothesis;

        public TestHandler(IHypothesis<T> hypothesis) => 
            _hypothesis = hypothesis;

        public Task Handle(T id) =>
            _hypothesis.Test(id);
    }

    private static async Task Publish()
    {
        using var client = new DaprClientBuilder()
            .UseGrpcEndpoint("http://localhost:3002")
            .Build();

        await client
            .PublishEventAsync("my-pubsub", "user/loggedIn",  new CloudEvent<Data>(new Data(1234)) { Type = "loggedIn"});
    }
}

internal record Data(int UserId);