using System;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using FluentAssertions;
using MassTransit.Topology.EntityNameFormatters;
using MassTransit.Topology.Topologies;
using MassTransit.Transports.InMemory.Configuration;
using MassTransit.Transports.InMemory.Contexts;
using MassTransit.Transports.InMemory.Fabric;
using Xunit;

namespace MassTransit.CloudEvents.Tests
{
    public class CloudEventDeserializerTests
    {
        [Fact]
        public void UseMessageIdFromCloudEvent()
        {
            var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
            {
                Data = "hello",
                Source = new Uri("https://google.nl"),
                Id = Guid.NewGuid().ToString(),
                Type = "my-custom-event"
            };

            Deserialize(cloudEvent)
                .MessageId
                .ToString()
                .Should()
                .Be(cloudEvent.Id);
        }

        [Fact]
        public void UseSourceAddressFromCloudEvent()
        {
            var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
            {
                Data = "hello",
                Source = new Uri("https://google.nl"),
                Id = Guid.NewGuid().ToString(),
                Type = "my-custom-event"
            };

            Deserialize(cloudEvent)
                .SourceAddress
                .Should()
                .Be(cloudEvent.Source);
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

            Deserialize(cloudEvent)
                .MessageId
                .Should()
                .BeNull();
        }

        private static ConsumeContext Deserialize(CloudEvent cloudEvent)
        {
            var formatter = new JsonEventFormatter();
            var message = formatter.EncodeStructuredModeMessage(cloudEvent, out _);

            using var context = NewReceiveContext(message);

            var serializer = new CloudEventsDeserializer().As<IMessageDeserializer>();
            return serializer.Deserialize(context);
        }

        private static InMemoryReceiveContext NewReceiveContext(ReadOnlyMemory<byte> message)
        {
            var topology = new InMemoryTopologyConfiguration(new MessageTopology(new MessageUrnEntityNameFormatter()));
            var bus = new InMemoryBusConfiguration(topology, new Uri("loopback://localhost"));
            var host = new InMemoryHostConfiguration(bus, new Uri("loopback://localhost"), topology);

            return new InMemoryReceiveContext(
                new InMemoryTransportMessage(Guid.Empty, message.ToArray(), "application/cloudevents+json", "my-custom-message"),
                new TransportInMemoryReceiveEndpointContext(host, new InMemoryReceiveEndpointConfiguration(host, "no-queue", bus)));
        }
    }
}
