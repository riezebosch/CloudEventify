using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentAssertions.Extensions;
using Hypothesist;
using MassTransit;
using Xunit;

namespace CloudEventify.MassTransit.Tests;

public class UseCloudEventsTests
{
    [Fact]
    public async Task WithJsonSerializerOptions()
    {
        // Arrange
        var hypothesis = Hypothesis.For<UserLoggedIn>()
            .Any(x => x.Id == 9999); // magic number injected by the custom converter.

        var converter = new CustomConverter();
        var bus = Bus.Factory.CreateUsingInMemory(cfg =>
        {
            cfg.UseCloudEvents()
                .WithJsonOptions(options => options.Converters.Add(converter));

            cfg.ReceiveEndpoint("test", 
                x => x.Consumer(hypothesis.AsConsumer));
        });

        await bus.StartAsync();

        // Act
        var endpoint = await bus.GetPublishSendEndpoint<UserLoggedIn>();
        await endpoint.Send(new UserLoggedIn(0));
            
        // Assert
        await hypothesis.Validate(2.Seconds());
    }

    private class CustomConverter : JsonConverter<UserLoggedIn>
    {
        public override UserLoggedIn Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => 
            new(reader.GetInt32());

        public override void Write(Utf8JsonWriter writer, UserLoggedIn value, JsonSerializerOptions options) => 
            writer.WriteNumberValue(9999);
    }

    private record UserLoggedIn(int Id);
}