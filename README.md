# CloudEvents[Rebus|MassTransit|Dapr]

[![nuget](https://img.shields.io/nuget/v/CloudEventify.svg)](https://www.nuget.org/packages/CloudEventify/)
[![codecov](https://codecov.io/gh/riezebosch/CloudEventify/branch/main/graph/badge.svg)](https://codecov.io/gh/riezebosch/CloudEventify)
[![stryker](https://img.shields.io/endpoint?style=flat&label=stryker&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Friezebosch%2FCloudEventify%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/riezebosch/CloudEventify/main)
[![build status](https://ci.appveyor.com/api/projects/status/a03ol21xakxbf477/branch/main?svg=true)](https://ci.appveyor.com/project/riezebosch/CloudEventify)

## TL;DR

> Just a serializer/deserializer for [cloud events](https://cloudevents.io/).

## Use CloudEvents

### Rebus + RabbitMQ

```c#
Configure.With(new EmptyActivator())
    .Transport(t => t.UseRabbitMqAsOneWayClient(_container.ConnectionString))
    .Serialization(s => s.UseCloudEvents()
        .WithTypes(types => types.Map<UserLoggedIn>("loggedIn")))
    .Start();
```

### MassTransit + RabbitMQ

On bus level:

```c#
var bus = Bus.Factory
    .CreateUsingRabbitMq(cfg =>
    {
        cfg.UseCloudEvents()
    };
```

On a specific receive endpoint:

```c#
var bus = Bus.Factory
    .CreateUsingRabbitMq(cfg =>
    {
        cfg.ReceiveEndpoint("...", x =>
        {
            x.UseCloudEvents();
        }
    };
```

This adds a _deserializer_ to support incoming messages using the default `application/cloudevents+json` content type **and**
sets the _serializer_ to wrap outgoing messages in a cloud event envelope.

## Message Types

All (custom) types must be explicitly mapped, both for outgoing and incoming messages.

```c#
.UseCloudEvents()
    .WithTypes(t => t.Map<UserLoggedIn>("loggedIn"));
```

Specify the `type` attribute on the cloud events envelope. 
Used by the deserializer when you want to deserialize to a specific (sub)type.

## Subject

The subject can be constructed using the instance of the outgoing message.

```c#
.UseCloudEvents()
    .WithTypes(t => t.Map<UserLoggedIn>("loggedIn"), map => map with { Subject = x => x.SomeProperty });
```

## Source 

For [Rebus](Rebus) you can also specify the `Source` attribute to be applied on the outgoing cloud event:

```c#
.Serialization(s => s.UseCloudEvents()
    .WithTypes(types => types.Map<UserLoggedIn>("user.loggedIn"))
    .WithSource(new System.Uri("uri:MySourceApp")))
```

Using [MassTransit](MassTransit) the `Source` attribute is used from the `SendContext`.

## Limitations

The use of cloud events is only developed for and tested in a pure pub/sub broker setup.
It is safe to assume that other patterns supported by MassTransit will not work since the information required for that is not conveyed.

## Interoperable

In the [integration tests](MassTransit/CloudEventify.MassTransit.IntegrationTests), [dapr](https://dapr.io) is used as publisher and subscriber to test both the serializer and deserializer. 