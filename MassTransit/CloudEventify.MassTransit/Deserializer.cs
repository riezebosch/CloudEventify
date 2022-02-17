using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using CloudNative.CloudEvents;
using GreenPipes;
using MassTransit;
using MassTransit.Context;

namespace CloudEventify.MassTransit;

public class Deserializer : IMessageDeserializer
{
    private readonly CloudEventFormatter _formatter;
    private readonly Unwrap _unwrap;

    public Deserializer(ContentType contentType, CloudEventFormatter formatter, Unwrap unwrap)
    {
        _formatter = formatter;
        _unwrap = unwrap;
        ContentType = contentType;
    }

    void IProbeSite.Probe(ProbeContext context)
    {
    }

    ConsumeContext IMessageDeserializer.Deserialize(ReceiveContext receiveContext) => 
        new CloudEventContext(receiveContext, _formatter.Decode(receiveContext.GetBody()), _unwrap);

    public ContentType ContentType
    {
        get;
    }

    private class CloudEventContext : DeserializerConsumeContext
    {
        private readonly CloudEvent _cloudEvent;
        private readonly Unwrap _unwrap;

        public CloudEventContext(ReceiveContext receiveContext, CloudEvent cloudEvent, Unwrap unwrap) : base(receiveContext)
        {
            _cloudEvent = cloudEvent;
            _unwrap = unwrap;
        }

        public override bool HasMessageType(Type messageType) =>
            true;

        public override bool TryGetMessage<T>(out ConsumeContext<T>? consumeContext)
        {
            try
            {
                consumeContext = new MessageConsumeContext<T>(this, (T)_unwrap.Envelope(_cloudEvent));
                return true;
            }
            catch (KeyNotFoundException)
            {
                consumeContext = null;
                return false;
            }
        }

        public override Guid? MessageId => Guid.TryParse(_cloudEvent.Id, out var result) ? result : null;
        public override Guid? RequestId => null;
        public override Guid? CorrelationId => null;
        public override Guid? ConversationId => null;
        public override Guid? InitiatorId => null;
        public override DateTime? ExpirationTime => null;
        public override Uri? SourceAddress => _cloudEvent.Source;
        public override Uri? DestinationAddress => null;
        public override Uri? ResponseAddress => null;
        public override Uri? FaultAddress => null;
        public override DateTime? SentTime => _cloudEvent.Time?.DateTime;
        public override Headers Headers { get; } = new DictionarySendHeaders(new Dictionary<string, object>());
        public override HostInfo? Host { get; }
        public override IEnumerable<string> SupportedMessageTypes { get; } = Enumerable.Empty<string>();
    }
}