using System;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;
using FluentAssertions.Extensions;

namespace CloudEventify.Rebus.IntegrationTests;

public sealed class RabbitMqContainer : IDisposable
{
    private readonly IContainerService _container;

    public RabbitMqContainer() =>
        _container = new Builder()
            .UseContainer()
            .UseImage("rabbitmq:alpine")
            .ExposePort(8395, 5672)
            .WaitForPort("5672/tcp", 30.Seconds())
            .WaitForMessageInLog("Server startup complete", 30.Seconds())
            .Build()
            .Start();

    public string ConnectionString =>  
        $"amqp://guest:guest@localhost:{_container.ToHostExposedEndpoint("5672/tcp").Port}"; // on linux the host exposed endpoint is: 0.0.0.0 for some reason

    void IDisposable.Dispose() => 
        _container.Dispose();
}