using System;
using System.Threading.Tasks;
using DaprApp;
using DaprApp.Controllers;
using FluentAssertions.Extensions;
using Hypothesist;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wrapr;
using Xunit;
using Xunit.Abstractions;

namespace CloudEventity.Dapr.IntegrationTests;

public class ToDapr
{
    private readonly ITestOutputHelper _output;

    public ToDapr(ITestOutputHelper output) => 
        _output = output;

    [Fact]
    public async Task Do()
    {
        // Arrange
        var message = new UserLoggedIn(1234);
        var hypothesis = Hypothesis
            .For<int>()
            .Any(x => x == message.UserId);

        await using var host = await Host(hypothesis.ToHandler());
        await using var sidecar = await Sidecar(_output);
            
        // Act
        await Publish(message);

        // Assert
        await hypothesis.Validate(10.Seconds());
    }

    private async Task Publish(UserLoggedIn message)
    {
        var client = new DaprClient("http://localhost:3001");
        await client.PublishEvent("my-pubsub", "user/loggedIn", message);
    }


    private static async Task<Sidecar> Sidecar(ITestOutputHelper logger)
    {
        var sidecar = new Sidecar("to-dapr", logger.ToLogger<Sidecar>());
        await sidecar.Start(with => with
            .ComponentsPath("components")
            .AppPort(6000)
            .DaprGrpcPort(3001));

        return sidecar;
    }

    private async Task<IAsyncDisposable> Host(IHandler<int> handler)
    {
        var app = Startup.App(builder =>
        {
            builder.Services.AddSingleton(handler);
            builder.Logging.AddXUnit(_output);
        });
        
        app.Urls.Add("http://localhost:6000");
        await app.StartAsync();
        
        return app;
    }

    public record UserLoggedIn(int UserId);
}