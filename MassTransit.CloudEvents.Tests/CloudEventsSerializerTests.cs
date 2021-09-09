using System.IO;
using System.Text.Json;
using Bogus;
using CloudNative.CloudEvents;
using FluentAssertions;
using MassTransit.Context;
using Xunit;

namespace MassTransit.CloudEvents.Tests
{
    public class CloudEventsSerializerTests
    {
        [Fact]
        public void UseMappingForType()
        {
            // Arrange
            var serializer = new CloudEventsSerializer();
            var message = new Faker<UserRegisteredEvent>()
                .StrictMode(true)
                .Generate();

            // Act
            using var stream = new MemoryStream();
            serializer.Serialize(stream, new MessageSendContext<UserRegisteredEvent>(message));

            // Assert
            JsonSerializer
                .Deserialize<CloudEvent>(stream.ToArray(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                .Type
                .Should()
                .Be("UserRegisteredEvent");
        }

        public class UserRegisteredEvent
        {
        }
    }
}
