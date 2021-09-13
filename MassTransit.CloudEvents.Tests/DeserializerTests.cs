using System;
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

namespace MassTransit.CloudEvents.Tests
{
    public class DeserializerTests
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

            using var context = ReceiveContext(cloudEvent.ToMessage());

            var serializer = new Deserializer().As<IMessageDeserializer>();
            serializer.Deserialize(context)
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

            using var context = ReceiveContext(cloudEvent.ToMessage());

            var serializer = new Deserializer().As<IMessageDeserializer>();
            serializer.Deserialize(context)
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

            using var context = ReceiveContext(cloudEvent.ToMessage());
            var serializer = new Deserializer().As<IMessageDeserializer>();
            serializer.Deserialize(context)
                .MessageId
                .Should()
                .BeNull();
        }
        
        [Fact]
        public void Unknown()
        {
            var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
            {
                Data = JsonSerializer.Deserialize<object>(@"{ ""id"": 1234 }"),
                Source = new Uri("https://google.nl"),
                Id = "1",
                Type = "my-custom-event"
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
        public void Mapped()
        {
            var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
            {
                Data = JsonSerializer.Deserialize<object>(@"{ ""id"": 1234 }"),
                Source = new Uri("https://google.nl"),
                Id = "1",
                Type = "user/loggedIn"
            };

            using var receive = ReceiveContext(cloudEvent.ToMessage());
            var deserializer = new Deserializer();
            deserializer.AddType<UserLoggedIn>("user/loggedIn");

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
                .Should()
                .Be(new UserLoggedIn(1234));
        }
        
        [Fact]
        public void Unmapped()
        {
            var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
            {
                Data = JsonSerializer.Deserialize<object>(@"{ ""id"": 1234 }"),
                Source = new Uri("https://google.nl"),
                Id = "1",
                Type = "user/loggedIn"
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
}
