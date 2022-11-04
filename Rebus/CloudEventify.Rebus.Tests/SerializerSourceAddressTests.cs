using AutoFixture;
using CloudNative.CloudEvents;
using FluentAssertions;
using Moq;
using Rebus.Config;
using Rebus.Messages;
using Rebus.Serialization;
using Rebus.Serialization.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CloudEventify.Rebus.Tests
{
    public class SerializerSourceAddressTests
    {
        private CloudEventFormatter _formatter = Formatter.New(new JsonSerializerOptions());
        private Mock<IMessageTypeNameConvention> _messageTypeNameConventionMock;
        private Message _simpleBaseMessage;

        public SerializerSourceAddressTests()
        {
            _messageTypeNameConventionMock = new Mock<IMessageTypeNameConvention>();
            _messageTypeNameConventionMock.Setup(m => m.GetTypeName(It.IsAny<Type>())).Returns("thing");
            _simpleBaseMessage = new Fixture().Build<Message>().Create();
            _simpleBaseMessage.Headers.Clear();
            _simpleBaseMessage.Headers.Add(Headers.MessageId, Guid.NewGuid().ToString());
            _simpleBaseMessage.Headers.Add(Headers.SentTime, DateTimeOffset.UtcNow.ToString("O"));
            _simpleBaseMessage.Headers.Add(Headers.Type, "thing");
            _simpleBaseMessage.Headers.Add(Headers.SenderAddress, "things");
            
        }


        [Fact]
        public async Task Serialize_Source_from_SoureAddress_override()
        {
            var fact = new Uri("app://my.app.com/things");

            var sut = (ISerializer)new Serializer(_formatter, new JsonSerializerOptions(), _messageTypeNameConventionMock.Object, fact);

            var res = await sut.Serialize(_simpleBaseMessage);

            res.Headers.Should().NotBeNullOrEmpty();
            res.Body.Should().NotBeNull();

            var cloudEvent = _formatter.Decode(res.Body);

            cloudEvent.Should().NotBeNull();
            cloudEvent.Source.Should().Be(fact);
        }

        [Fact]
        public async Task Serialize_Source_from_SenderAddress_header()
        {
            var sut = (ISerializer)new Serializer(_formatter, new JsonSerializerOptions(), _messageTypeNameConventionMock.Object, null);

            var res = await sut.Serialize(_simpleBaseMessage);

            res.Headers.Should().NotBeNullOrEmpty();
            res.Body.Should().NotBeNull();

            var cloudEvent = _formatter.Decode(res.Body);

            cloudEvent.Should().NotBeNull();
            cloudEvent.Source.Should().Be($"cloudeventify://rebus.queue.things");
        }

        [Fact]
        public async Task Serialize_Source_from_fallback_default()
        {
            var sut = (ISerializer)new Serializer(_formatter, new JsonSerializerOptions(), _messageTypeNameConventionMock.Object, null);

            if (_simpleBaseMessage.Headers.ContainsKey(Headers.SenderAddress))
            {
                _simpleBaseMessage.Headers.Remove(Headers.SenderAddress);
            }

            var res = await sut.Serialize(_simpleBaseMessage);

            res.Headers.Should().NotBeNullOrEmpty();
            res.Body.Should().NotBeNull();

            var cloudEvent = _formatter.Decode(res.Body);

            cloudEvent.Should().NotBeNull();
            cloudEvent.Source.Should().Be($"cloudeventify://rebus");
        }


    }
}
