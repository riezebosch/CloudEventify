using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CloudNative.CloudEvents;
using FluentAssertions;
using MassTransit.Context;
using MassTransit.Topology.EntityNameFormatters;
using MassTransit.Topology.Topologies;
using MassTransit.Transports.InMemory.Configuration;
using MassTransit.Transports.InMemory.Contexts;
using MassTransit.Transports.InMemory.Fabric;
using Xunit;

namespace MassTransit.CloudEvents.Tests;

public class DeserializerTests
{
    [Fact]
    public void UnusedPropertiesMapToDefaults()
    {
        // Arrange
        var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
        {
            Data = "hello",
            Source = new Uri("https://google.nl"),
            Id = "1",
            Type = "my-custom-event"
        };

        using var receive = ReceiveContext(cloudEvent.ToMessage());
        var serializer = new Deserializer().As<IMessageDeserializer>();
            
        // Act
        var consume = serializer.Deserialize(receive);
            
        // Assert
        consume
            .Should()
            .BeEquivalentTo(new
            {
                MessageId = (Guid?)null,
                RequestId = (Guid?)null,
                CorrelationId = (Guid?)null,
                ConversationId = (Guid?)null,
                InitiatorId = (Guid?)null,
                ExpirationTime = (DateTime?)null,
                DestinationAddress = (Uri)null,
                ResponseAddress = (Uri)null,
                FaultAddress = (Uri)null,
                Headers = (Headers)null,
                Host = (HostInfo)null,
                SupportedMessageTypes = Enumerable.Empty<string>()
            });
    }
        
    [Fact]
    public void UseMessageIdFromCloudEvent()
    {
        var id = Guid.NewGuid();
        var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
        {
            Data = "hello",
            Source = new Uri("https://google.nl"),
            Id = id.ToString(),
            Type = "my-custom-event"
        };

        using var context = ReceiveContext(cloudEvent.ToMessage());

        var serializer = new Deserializer().As<IMessageDeserializer>();
        serializer.Deserialize(context)
            .MessageId
            .Should()
            .Be(id);
    }

    [Fact]
    public void UseSourceAddressFromCloudEvent()
    {
        var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
        {
            Data = "hello",
            Source = new Uri("https://google.nl"),
            Id = "1",
            Type = "my-custom-event"
        };

        using var context = ReceiveContext(cloudEvent.ToMessage());

        var serializer = new Deserializer().As<IMessageDeserializer>();
        serializer.Deserialize(context)
            .SourceAddress
            .Should()
            .Be(cloudEvent.Source);
    }

    [Fact]
    public void UseSentTime()
    {
        var sent = new DateTimeOffset(2001, 1, 3, 14, 21, 5, TimeSpan.FromHours(2));
        var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
        {
            Data = "hello",
            Source = new Uri("https://google.nl"),
            Id = Guid.NewGuid().ToString(),
            Type = "my-custom-event",
            Time = sent
        };

        using var context = ReceiveContext(cloudEvent.ToMessage());

        var serializer = new Deserializer().As<IMessageDeserializer>();
        serializer.Deserialize(context)
            .SentTime
            .Should()
            .Be(sent.DateTime);
    }

    [Fact]
    public void IdFromCloudEventIsNoGuid_MessageIdIsNull()
    {
        var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
        {
            Data = "hello",
            Source = new Uri("https://google.nl"),
            Id = "1",
            Type = "my-custom-event"
        };

        using var context = ReceiveContext(cloudEvent.ToMessage());
        var serializer = new Deserializer().As<IMessageDeserializer>();
        serializer.Deserialize(context)
            .MessageId
            .Should()
            .BeNull();
    }
        
    [Fact]
    public void SpecificTypeNotMapped()
    {
        var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
        {
            Data = new { id =  1234 },
            Source = new Uri("https://google.nl"),
            Id = "1",
            Type = "ignored"
        };

        using var receive = ReceiveContext(cloudEvent.ToMessage());
        var context = new Deserializer()
            .As<IMessageDeserializer>()
            .Deserialize(receive)
            .As<DeserializerConsumeContext>();
            
        context
            .TryGetMessage<UserLoggedIn>(out var consume)
            .Should()
            .BeTrue();
            
        consume
            .Message
            .Should()
            .Be(new UserLoggedIn(1234));
    }
        
    [Fact]
    public void FromInterface()
    {
        var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
        {
            Data = new { id = 1234 },
            Source = new Uri("https://google.nl"),
            Id = "1",
            Type = "user/loggedIn"
        };

        using var receive = ReceiveContext(cloudEvent.ToMessage());
        var deserializer = new Deserializer();
        deserializer            
            .AddType<UserLoggedIn>("user/loggedIn");

        var context = deserializer
            .As<IMessageDeserializer>()
            .Deserialize(receive)
            .As<DeserializerConsumeContext>();
            
        context
            .TryGetMessage<IEvent>(out var consume)
            .Should()
            .BeTrue();
            
        consume
            .Message
            .Id
            .Should()
            .Be(1234);
    }
        
    [Fact]
    public void FromInterfaceUnmapped()
    {
        var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
        {
            Data = new { Id = 1234 },
            Source = new Uri("https://google.nl"),
            Id = "1",
            Type = "unmapped"
        };

        using var receive = ReceiveContext(cloudEvent.ToMessage());
        var context = new Deserializer()
            .As<IMessageDeserializer>()
            .Deserialize(receive)
            .As<DeserializerConsumeContext>();
            
        context
            .TryGetMessage<IEvent>(out _)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void PretendToHaveAllMessageTypes()
    {
        var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
        {
            Data = "hello",
            Source = new Uri("https://google.nl"),
            Id = "1",
            Type = "user/loggedIn"
        };

        using var receive = ReceiveContext(cloudEvent.ToMessage());
        var consume = new Deserializer()
            .As<IMessageDeserializer>()
            .Deserialize(receive);
            
        consume
            .HasMessageType(typeof(int))
            .Should()
            .BeTrue();
    }

    private interface IEvent
    {
        int Id { get; }
    }

    private record UserLoggedIn(int Id) : IEvent;

    private static InMemoryReceiveContext ReceiveContext(ReadOnlyMemory<byte> message)
    {
        var topology = new InMemoryTopologyConfiguration(new MessageTopology(new MessageUrnEntityNameFormatter()));
        var bus = new InMemoryBusConfiguration(topology, new Uri("loopback://localhost"));
        var host = new InMemoryHostConfiguration(bus, new Uri("loopback://localhost"), topology);

        return new InMemoryReceiveContext(
            new InMemoryTransportMessage(Guid.Empty, message.ToArray(), "application/cloudevents+json", "my-custom-message"),
            new TransportInMemoryReceiveEndpointContext(host, new InMemoryReceiveEndpointConfiguration(host, "no-queue", bus)));
    }
}