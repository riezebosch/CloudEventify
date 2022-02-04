using System;
using System.IO;
using System.Text.Json;
using Bogus;
using CloudNative.CloudEvents;
using FluentAssertions;
using MassTransit.Context;
using Xunit;

namespace MassTransit.CloudEvents.Tests;

public class SerializerTests
{
    private readonly Faker<UserRegisteredEvent> _faker = new Faker<UserRegisteredEvent>().StrictMode(true);

    [Fact]
    public void UseType()
    {
        // Arrange
        var serializer = new Serializer();
        var message = _faker.Generate();

        // Act
        using var stream = new MemoryStream();
        serializer.Serialize(stream, new MessageSendContext<UserRegisteredEvent>(message));

        // Assert
        Deserialize(stream)
            .Type
            .Should()
            .Be("UserRegisteredEvent");
    }
        
    [Fact]
    public void UseMappingForType()
    {
        // Arrange
        var serializer = new Serializer();
        serializer
            .AddType<UserRegisteredEvent>("registered");
            
        var message = _faker.Generate();

        // Act
        using var stream = new MemoryStream();
        serializer.Serialize(stream, new MessageSendContext<UserRegisteredEvent>(message));

        // Assert
        Deserialize(stream)
            .Type
            .Should()
            .Be("registered");
    }
        
    [Fact]
    public void UseSourceAddress()
    {
        // Arrange
        var serializer = new Serializer();
        var message = _faker.Generate();

        // Act
        using var stream = new MemoryStream();
        var source = new Uri("https://github.com/cloudevents");
        serializer.Serialize(stream, new MessageSendContext<UserRegisteredEvent>(message)
        {
            SourceAddress = source,
        });

        // Assert
        Deserialize(stream)
            .Source
            .Should()
            .Be(source);
    }
        
    [Fact]
    public void UseSentTime()
    {
        // Arrange
        var serializer = new Serializer();
        var message = _faker.Generate();
        var context = new MessageSendContext<UserRegisteredEvent>(message);

        // Act
        using var stream = new MemoryStream();
        serializer.Serialize(stream, context);

        // Assert
        Deserialize(stream)
            .Time
            .Should()
            .NotBeNull()
            .And
            .Be(context.SentTime);
    }

    private static CloudEvent Deserialize(MemoryStream stream) =>
        JsonSerializer.Deserialize<CloudEvent>(stream.ToArray(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    private record UserRegisteredEvent;
}