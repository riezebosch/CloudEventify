using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Bogus;
using CloudNative.CloudEvents;
using FluentAssertions;
using MassTransit.Context;
using Xunit;

namespace CloudEventify.MassTransit.Tests;

public class SerializerTests
{
    private readonly Faker<UserRegisteredEvent> _faker = new Faker<UserRegisteredEvent>().StrictMode(true);
    private readonly CloudEventFormatter _formatter = Formatter.New(new JsonSerializerOptions());

    [Fact]
    public void Unmapped()
    {
        // Arrange
        var serializer = Serializer(types => types);

        // Act
        var act = () => serializer.Serialize(null!, new MessageSendContext<string>(""));

        // Assert
        act.Should().Throw<KeyNotFoundException>();
    }
        
    [Fact]
    public void UseMappingForType()
    {
        // Arrange
        var serializer = Serializer(types => types.Map<UserRegisteredEvent>("registered"));
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
        var serializer = Serializer(types => types.Map<UserRegisteredEvent>("asdf"));
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
        var message = _faker.Generate();
        var context = new MessageSendContext<UserRegisteredEvent>(message);

        // Act
        using var stream = new MemoryStream();
        var serializer = Serializer(types => types.Map<UserRegisteredEvent>("asdf"));
        serializer.Serialize(stream, context);

        // Assert
        Deserialize(stream)
            .Time
            .Should()
            .NotBeNull()
            .And
            .Be(context.SentTime);
    }

    private Serializer Serializer(Func<ITypesMap, ITypesMap> map) => new(null!, _formatter, new Wrap(map(new TypesMapper())));

    private static CloudEvent Deserialize(MemoryStream stream) =>
        JsonSerializer.Deserialize<CloudEvent>(stream.ToArray(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    private record UserRegisteredEvent;
}