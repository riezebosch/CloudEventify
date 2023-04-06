using System;
using System.Threading.Tasks;
using Dapr.Client;
using DaprApp;
using DaprApp.Controllers;
using FluentAssertions.Extensions;
using Hypothesist;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wrapr;
using Xunit;
using Xunit.Abstractions;

namespace CloudEventify.Dapr.IntegrationTests;

public class ToDapr
{
    private readonly ITestOutputHelper _output;
    private const int AppPort = 6000;
    private const int DaprGrpcPort = 3001;
    private const string DaprHttpPort = "3002";

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
        await hypothesis.Validate(30.Seconds());
    }

    private static async Task Publish(UserLoggedIn message)
    {
        using var client = new DaprClientBuilder()
            .UseGrpcEndpoint($"http://127.0.0.1:{DaprGrpcPort}")
            .UseHttpEndpoint($"http://127.0.0.1:{DaprHttpPort}")
            .UseCloudEvents()
            .WithTypes(types => types.Map<UserLoggedIn>("loggedIn"))
            .Build();
        
        await client.WaitForSidecarAsync();
        await client.PublishEventAsync("my-pubsub", "user/loggedIn", message);
    }


    private async Task<Sidecar> Sidecar(ITestOutputHelper logger)
    {
        var sidecar = new Sidecar("to-dapr", logger.ToLogger<Sidecar>());
        await sidecar.Start(with => with
            .ComponentsPath("components")
            .AppPort(AppPort)
            .Args("-H", DaprHttpPort)
            .DaprGrpcPort(DaprGrpcPort));

        return sidecar;
    }

    private async Task<IAsyncDisposable> Host(IHandler<int> handler)
    {
        var app = Startup.App(builder =>
        {
            builder.Services.AddSingleton(handler);
            builder.Logging.AddXUnit(_output);
        });

        app.Urls.Add($"http://localhost:{AppPort}");
        await app.StartAsync();
        
        return app;
    }

    public record UserLoggedIn(int UserId);
}