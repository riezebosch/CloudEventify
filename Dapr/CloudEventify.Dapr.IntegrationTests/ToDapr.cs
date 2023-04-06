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

        await using var host = await Host(hypothesis.ToHandler(), 6000);
        await using var sidecar = await Sidecar(6000, 3001, 3000, _output);
            
        // Act
        await Publish(message, 3001, 3000);

        // Assert
        await hypothesis.Validate(30.Seconds());
    }

    private static async Task Publish(UserLoggedIn message, int grpc, int http)
    {
        using var client = new DaprClientBuilder()
            .UseGrpcEndpoint($"http://127.0.0.1:{grpc}")
            .UseHttpEndpoint($"http://127.0.0.1:{http}")
            .UseCloudEvents()
            .WithTypes(types => types.Map<UserLoggedIn>("loggedIn"))
            .Build();
        
        await client.WaitForSidecarAsync();
        await client.PublishEventAsync("my-pubsub", "user/loggedIn", message);
    }

    private static async Task<Sidecar> Sidecar(int app, int grpc, int http, ITestOutputHelper output)
    {
        var sidecar = new Sidecar("to-dapr", output.ToLogger<Sidecar>());
        await sidecar.Start(with => with
            .ResourcesPath("components")
            .AppPort(app)
            .DaprHttpPort(http)
            .DaprGrpcPort(grpc));

        return sidecar;
    }

    private async Task<IAsyncDisposable> Host(IHandler<int> handler, int port)
    {
        var app = Startup.App(builder =>
        {
            builder.Services.AddSingleton(handler);
            builder.Logging.AddXUnit(_output);
        });

        app.Urls.Add($"http://localhost:{port}");
        await app.StartAsync();
        
        return app;
    }

    public record UserLoggedIn(int UserId);
}