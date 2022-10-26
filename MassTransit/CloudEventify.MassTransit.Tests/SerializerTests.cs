using System;
using System.Collections.Generic;
using System.Text.Json;
using Bogus;
using CloudNative.CloudEvents;
using FluentAssertions;
using MassTransit;
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
        var act = () => serializer.GetMessageBody(new MessageSendContext<string>(""));

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
        var body = serializer.GetMessageBody(new MessageSendContext<UserRegisteredEvent>(message));

        // Assert
        Deserialize(body.GetBytes())
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
        var source = new Uri("https://github.com/cloudevents");
        var body = serializer.GetMessageBody(new MessageSendContext<UserRegisteredEvent>(message)
        {
            SourceAddress = source,
        });

        // Assert
        Deserialize(body.GetBytes())
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
        var serializer = Serializer(types => types.Map<UserRegisteredEvent>("asdf"));
        var body = serializer.GetMessageBody(context);

        // Assert
        Deserialize(body.GetBytes())
            .Time
            .Should()
            .NotBeNull()
            .And
            .Be(context.SentTime);
    }
    
    [Fact]
    public void UseSubject()
    {
        // Arrange
        var message = _faker.Generate();
        var context = new MessageSendContext<UserRegisteredEvent>(message);

        // Act
        var serializer = Serializer(types => types.Map<UserRegisteredEvent>("asdf", m => m with{ Subject = _ => "efgh"}));
        var body = serializer.GetMessageBody(context);

        // Assert
        Deserialize(body.GetBytes())
            .Subject
            .Should()
            .Be("efgh");
    }

    private IMessageSerializer Serializer(Func<IMap, IMap> map) => 
        new Serializer(null!, _formatter, new Wrap(map(new Mapper())));

    private static CloudEvent Deserialize(byte[] body) =>
        JsonSerializer.Deserialize<CloudEvent>(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    private record UserRegisteredEvent;
}