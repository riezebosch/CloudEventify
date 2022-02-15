using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using GreenPipes;
using MassTransit;
using MassTransit.Context;

namespace CloudEventify.MassTransit;

public class Deserializer : IMessageDeserializer
{
    private readonly Dictionary<string, Type> _types = new ();

    void IProbeSite.Probe(ProbeContext context)
    {
    }

    ConsumeContext IMessageDeserializer.Deserialize(ReceiveContext receiveContext)
    {
        var formatter = new JsonEventFormatter();
        var message = formatter.DecodeStructuredModeMessage((ReadOnlyMemory<byte>)receiveContext.GetBody(), null, null);

        return new CloudEventContext(receiveContext, message, _types, Options);
    }

    public ContentType ContentType
    {
        get;
        set; 
    } = new ("application/cloudevents+json");

    public JsonSerializerOptions Options { get; } = new() { PropertyNameCaseInsensitive = true };


    public void AddType<T>(string type) => 
        _types[type] = typeof(T);

    private class CloudEventContext : DeserializerConsumeContext
    {
        private readonly CloudEvent _cloudEvent;
        private readonly Dictionary<string, Type> _mappings;
        private readonly JsonSerializerOptions _options;

        public CloudEventContext(ReceiveContext receiveContext, CloudEvent cloudEvent,
            Dictionary<string, Type> mappings, JsonSerializerOptions options) 
            : base(receiveContext)
        {
            _cloudEvent = cloudEvent;
            _mappings = mappings;
            _options = options;
        }

        public override bool HasMessageType(Type messageType) =>
            true;

        public override bool TryGetMessage<T>(out ConsumeContext<T>? consumeContext)
        {
            try
            {
                var message = _cloudEvent.ToObject<T>(Type<T>(), _options);
                consumeContext = new MessageConsumeContext<T>(this, message);
                return true;
            }
            catch (NotSupportedException)
            {
                consumeContext = null;
                return false;
            }
        }

        public override Guid? MessageId => Guid.TryParse(_cloudEvent.Id, out var result) ? result : null;
        public override Guid? RequestId { get; }
        public override Guid? CorrelationId { get; }
        public override Guid? ConversationId { get; }
        public override Guid? InitiatorId { get; }
        public override DateTime? ExpirationTime { get; }
        public override Uri? SourceAddress => _cloudEvent.Source;
        public override Uri? DestinationAddress { get; }
        public override Uri? ResponseAddress { get; }
        public override Uri? FaultAddress { get; }
        public override DateTime? SentTime => _cloudEvent.Time?.DateTime;
        public override Headers? Headers { get; } 
        public override HostInfo? Host { get; }
        public override IEnumerable<string> SupportedMessageTypes { get; } = Enumerable.Empty<string>();

        private Type Type<T>() => 
            _mappings.TryGetValue(_cloudEvent.Type!, out var result) ? result : typeof(T);
    }
}