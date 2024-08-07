using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CloudNative.CloudEvents;
using FluentAssertions;
using MassTransit;
using MassTransit.Context;
using MassTransit.InMemoryTransport;
using MassTransit.InMemoryTransport.Configuration;
using MassTransit.Serialization;
using MassTransit.Topology;
using Xunit;

namespace CloudEventify.MassTransit.Tests;

public class DeserializerTests
{
    private readonly CloudEventFormatter _formatter = Formatter.New(new JsonSerializerOptions
        { PropertyNameCaseInsensitive = true });
    
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

        using var receive = ReceiveContext(_formatter.Encode(cloudEvent));
        var serializer = Serializer(types => types.Map<string>("my-custom-event"));
            
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
                Headers = new DictionarySendHeaders(new Dictionary<string, object>()),
                Host = (HostInfo)null,
                SupportedMessageTypes = Enumerable.Empty<string>()
            });
    }

    private static IMessageDeserializer Serializer(Func<IMap, IMap> map)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return new Deserializer(null!, Formatter.New(options), new Unwrap(map(new Mapper()), options), null!);
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

        using var context = ReceiveContext(_formatter.Encode(cloudEvent));
        
        var serializer = Serializer(types => types.Map<string>("my-custom-event"));
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

        using var context = ReceiveContext(_formatter.Encode(cloudEvent));
        var serializer = Serializer(types => types.Map<string>("my-custom-event"));
        serializer.Deserialize(context)
            .SourceAddress
            .Should()
            .Be("https://google.nl/");
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

        using var context = ReceiveContext(_formatter.Encode(cloudEvent));

        var serializer = Serializer(types => types.Map<string>("my-custom-event"));
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

        using var context = ReceiveContext(_formatter.Encode(cloudEvent));
        var serializer = Serializer(types => types.Map<string>("my-custom-event"));
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

        using var receive = ReceiveContext(_formatter.Encode(cloudEvent));
        var context = Serializer(types => types)
            .Deserialize(receive)
            .As<DeserializerConsumeContext>();
            
        context
            .TryGetMessage<UserLoggedIn>(out _)
            .Should()
            .BeFalse();
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

        using var receive = ReceiveContext(_formatter.Encode(cloudEvent));
        var serializer = Serializer(types => types.Map<UserLoggedIn>("user/loggedIn"));
        var context = serializer
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

        using var receive = ReceiveContext(_formatter.Encode(cloudEvent));
        var context = Serializer(types => types)
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

        using var receive = ReceiveContext(_formatter.Encode(cloudEvent));
        var consume = Serializer(types => types.Map<string>("my-custom-event"))
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
            new InMemoryTransportMessage(Guid.Empty, message.ToArray(), "application/cloudevents+json"),
            new TransportInMemoryReceiveEndpointContext(host, new InMemoryReceiveEndpointConfiguration(host, "no-queue", bus)));
    }
}