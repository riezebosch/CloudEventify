# MassTransit+CloudEvents

[![nuget](https://img.shields.io/nuget/v/CloudEventify.MassTransit.svg)](https://www.nuget.org/packages/CloudEventify.MassTransit/)
[![codecov](https://codecov.io/gh/riezebosch/MassTransit.CloudEvents/branch/main/graph/badge.svg)](https://codecov.io/gh/riezebosch/MassTransit.CloudEvents)
[![build status](https://ci.appveyor.com/api/projects/status/a03ol21xakxbf477/branch/main?svg=true)](https://ci.appveyor.com/project/riezebosch/masstransit-cloudevents)

## TL;DR

> Just a serializer/deserializer for [cloud events](https://cloudevents.io/).

## Use CloudEvents

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

## Content Type

```c#
cfg.UseCloudEvents()
    .WithContentType(new ContentType("text/plain"));
```

Sets the content-type for both the serializer _and_ the deserializer.
For example when the publishing side chooses [a different content type](https://github.com/dapr/components-contrib/blob/master/bindings/rabbitmq/rabbitmq.go#L98).

You can invoke the `UseCloudEvents` with a different `ContentType` multiple times
but the last one wins for the outbound (serializer) configuration.

## Message Types

```c#
cfg.UseCloudEvents()
    .Type<UserLoggedIn>("loggedIn");
```

Specify the `type` attribute on the cloud events envelope. 
Used by the deserializer when you want to deserialize to a specific subtype.

## Limitations

The use of cloud events is only developed for and tested in a pure pub/sub broker setup.
It is safe to assume that other patterns supported by MassTransit will not work since the information required for that is not conveyed.

## Interoperable

In the [integration tests](MassTransit.CloudEvents.IntegrationTests), [dapr](https://dapr.io) is used as publisher and subscriber to test both the serializer and deserializer. 