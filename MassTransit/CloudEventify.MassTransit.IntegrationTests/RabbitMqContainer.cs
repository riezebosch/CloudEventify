using System;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;

namespace CloudEventify.MassTransit.IntegrationTests;

public sealed class RabbitMqContainer : IDisposable
{
    private readonly IContainerService _container;

    public RabbitMqContainer() =>
        _container = new Builder().UseContainer()
            .UseImage("rabbitmq:alpine")
            .ExposePort(8394, 5672)
            .WaitForPort("5672/tcp", 30000 /*30s*/)
            .WaitForMessageInLog("Server startup complete")
            .Build()
            .Start();

    public string ConnectionString => $"amqp://guest:guest@localhost:{_container.ToHostExposedEndpoint("5672/tcp").Port}"; // on linux the host exposed endpoint is: 0.0.0.0 for some reason

    void IDisposable.Dispose() => 
        _container.Dispose();
}