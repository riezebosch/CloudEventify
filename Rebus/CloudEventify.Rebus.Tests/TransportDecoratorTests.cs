using CloudNative.CloudEvents;
using FluentAssertions;
using Moq;
using Rebus.Messages;
using Rebus.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CloudEventify.Rebus.Tests
{
    public class TransportDecoratorTests
    {
        [Fact]
        public async void Receive_Should_Take_CloudEvent_Id_and_Set_Rbs2_msg_id_header()
        {
            var fact = GetDummyCloudEvent();

            TransportDecorator sut = GetTransportDecorator(fact);

            var message = await sut.Receive(null!, CancellationToken.None);

            message!.Headers[Headers.MessageId].Should().Be(fact.Id);
        }


        [Fact]
        public async void Receive_Should_Take_SetHeaders_From_Attributes()
        {
            CloudEvent fact = GetDummyCloudEvent();

            //Don't want to override the messageid .. maybe it should be removed all together..
            var allHeadersFact = HeaderMap.Instance.Reverse.Items.Where(i => i.Value != Headers.MessageId).ToDictionary(a => a.Key, a => Guid.NewGuid().ToString());
            foreach (var item in allHeadersFact)
            {
                fact.SetAttributeFromString(item.Key, item.Value);
            }

            TransportDecorator sut = GetTransportDecorator(fact);

            var message = await sut.Receive(null!, CancellationToken.None);

            message!.Headers[Headers.MessageId].Should().Be(fact.Id);

            foreach (var item in HeaderMap.Instance.Forward.Items.Where(i => i.Key != Headers.MessageId))
            {
                message.Headers.Keys.Should().Contain(item.Key);
                message.Headers[item.Key].Should().Be(allHeadersFact[item.Value]);
            }
        }

        private static CloudEvent GetDummyCloudEvent()
        {
            return new CloudEvent(CloudEventsSpecVersion.V1_0)
            {
                Id = Guid.NewGuid().ToString(),
                Subject = "somesubject",
                Source = new Uri("app://some.source.com"),
                Time = DateTime.UtcNow,
                DataContentType = "application/json",
                DataSchema = new Uri("app://some.schema.com"),
                Type = "dummy",
            };
        }

        private static TransportDecorator GetTransportDecorator(CloudEvent fact)
        {
            var cloudEventBytes = Formatter.New(new JsonSerializerOptions(JsonSerializerDefaults.General))
                .EncodeStructuredModeMessage(fact, out var contentType).ToArray();

            var transportMock = new Mock<ITransport>();
            transportMock.Setup(t => t.Receive(It.IsAny<ITransactionContext>(), It.IsAny<CancellationToken>())).ReturnsAsync(new TransportMessage(new Dictionary<string, string>(), cloudEventBytes));

            return new TransportDecorator(transportMock.Object);
        }
    }
}
