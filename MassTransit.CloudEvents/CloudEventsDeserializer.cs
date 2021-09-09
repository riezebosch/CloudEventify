using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using GreenPipes;
using MassTransit.Context;

namespace MassTransit.CloudEvents
{
    public class CloudEventsDeserializer : IMessageDeserializer
    {
        void IProbeSite.Probe(ProbeContext context)
        {
        }

        ConsumeContext IMessageDeserializer.Deserialize(ReceiveContext receiveContext)
        {
            var formatter = new JsonEventFormatter();
            var message = formatter.DecodeStructuredModeMessage((ReadOnlyMemory<byte>)receiveContext.GetBody(), null, null);

            return new CloudEventContext(receiveContext, message);
        }

        private class CloudEventContext : DeserializerConsumeContext
        {
            private readonly CloudEvent _cloudEvent;

            public CloudEventContext(ReceiveContext receiveContext, CloudEvent cloudEvent) : base(receiveContext) =>
                _cloudEvent = cloudEvent;

            public override bool HasMessageType(Type messageType) =>
                true;

            public override bool TryGetMessage<T>(out ConsumeContext<T> consumeContext)
            {
                consumeContext = new MessageConsumeContext<T>(this, ((JsonElement)_cloudEvent.Data).ToObject<T>(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                return true;
            }

            public override Guid? MessageId => Guid.TryParse(_cloudEvent.Id, out var result) ? result : null;
            public override Guid? RequestId { get; }
            public override Guid? CorrelationId { get; }
            public override Guid? ConversationId { get; }
            public override Guid? InitiatorId { get; }
            public override DateTime? ExpirationTime { get; }
            public override Uri SourceAddress => _cloudEvent.Source;
            public override Uri DestinationAddress { get; }
            public override Uri ResponseAddress { get; }
            public override Uri FaultAddress { get; }
            public override DateTime? SentTime { get; }
            public override Headers Headers { get; }
            public override HostInfo Host { get; }
            public override IEnumerable<string> SupportedMessageTypes { get; }
        }

        public ContentType ContentType
        {
            get;
            set; 
        } = new ("text/plain");
    }
}
