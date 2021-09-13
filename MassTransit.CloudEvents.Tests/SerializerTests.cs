using System;
using System.IO;
using System.Text.Json;
using Bogus;
using CloudNative.CloudEvents;
using FluentAssertions;
using MassTransit.Context;
using Xunit;

namespace MassTransit.CloudEvents.Tests
{
    public class SerializerTests
    {
        [Fact]
        public void UseType()
        {
            // Arrange
            var serializer = new Serializer();
            var message = new Faker<UserRegisteredEvent>()
                .StrictMode(true)
                .Generate();

            // Act
            using var stream = new MemoryStream();
            serializer.Serialize(stream, new MessageSendContext<UserRegisteredEvent>(message));

            // Assert
            JsonSerializer
                .Deserialize<CloudEvent>(stream.ToArray(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!
                .Type
                .Should()
                .Be("UserRegisteredEvent");
        }
        
        [Fact]
        public void UseMappingForType()
        {
            // Arrange
            var serializer = new Serializer();
            serializer.AddType<UserRegisteredEvent>("registered");
            
            var message = new Faker<UserRegisteredEvent>()
                .StrictMode(true)
                .Generate();

            // Act
            using var stream = new MemoryStream();
            serializer.Serialize(stream, new MessageSendContext<UserRegisteredEvent>(message));

            // Assert
            JsonSerializer
                .Deserialize<CloudEvent>(stream.ToArray(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!
                .Type
                .Should()
                .Be("registered");
        }
        
        [Fact]
        public void UseSourceAddress()
        {
            // Arrange
            var serializer = new Serializer();
            serializer.AddType<UserRegisteredEvent>("registered");
            
            var message = new Faker<UserRegisteredEvent>()
                .StrictMode(true)
                .Generate();

            // Act
            using var stream = new MemoryStream();
            var source = new Uri("https://github.com/cloudevents");
            serializer.Serialize(stream, new MessageSendContext<UserRegisteredEvent>(message) { SourceAddress = source});

            // Assert
            JsonSerializer
                .Deserialize<CloudEvent>(stream.ToArray(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!
                .Source
                .Should()
                .Be(source);
        }

        private record UserRegisteredEvent;
    }
}
