using System;
using System.Collections.Generic;
using System.Net.Mime;
using CloudNative.CloudEvents;
using MassTransit;
using MassTransit.Context;
using MassTransit.Serialization;

namespace CloudEventify.MassTransit;

public class Deserializer : IMessageDeserializer
{
    private readonly ISerializerFactory _factory;
    private readonly CloudEventFormatter _formatter;
    private readonly Unwrap _unwrap;

    public Deserializer(ContentType contentType, CloudEventFormatter formatter, Unwrap unwrap, ISerializerFactory factory)
    {
        _formatter = formatter;
        _unwrap = unwrap;
        _factory = factory;
        ContentType = contentType;
    }

    void IProbeSite.Probe(ProbeContext context)
    {
    }

    ConsumeContext IMessageDeserializer.Deserialize(ReceiveContext receiveContext) => 
        new CloudEventContext(receiveContext, Deserialize(receiveContext.Body, receiveContext.TransportHeaders, receiveContext.InputAddress));

    public SerializerContext Deserialize(MessageBody body, Headers headers, Uri? destinationAddress) =>
        new Context(_formatter.Decode(body.GetBytes()), _unwrap, _factory);

    private class Context : SerializerContext
    {
        private readonly CloudEvent _cloudEvent;
        private readonly Unwrap _unwrap;
        private readonly ISerializerFactory _factory;

        public Context(CloudEvent cloudEvent, Unwrap unwrap, ISerializerFactory factory)
        {
            _cloudEvent = cloudEvent;
            _unwrap = unwrap;
            _factory = factory;
        }

        Guid? MessageContext.MessageId => Guid.TryParse(_cloudEvent.Id, out var id) ? id : null;

        Guid? MessageContext.RequestId { get; }

        Guid? MessageContext.CorrelationId { get; }

        Guid? MessageContext.ConversationId { get; }

        Guid? MessageContext.InitiatorId { get; }

        DateTime? MessageContext.ExpirationTime { get; }

        Uri? MessageContext.SourceAddress => _cloudEvent.Source;

        Uri? MessageContext.DestinationAddress { get; }

        Uri? MessageContext.ResponseAddress { get; }

        Uri? MessageContext.FaultAddress { get; }

        DateTime? MessageContext.SentTime => _cloudEvent.Time?.DateTime;

        Headers MessageContext.Headers => EmptyHeaders.Instance;

        HostInfo MessageContext.Host { get; }

        T IObjectDeserializer.DeserializeObject<T>(object? value, T? defaultValue) where T : class => 
            throw new NotImplementedException();

        T? IObjectDeserializer.DeserializeObject<T>(object? value, T? defaultValue) => 
            throw new NotImplementedException();

        MessageBody IObjectDeserializer.SerializeObject(object? value) => 
            throw new NotImplementedException();

        bool SerializerContext.IsSupportedMessageType<T>() => 
            true;

        public bool IsSupportedMessageType(Type messageType) => 
            true;

        bool SerializerContext.TryGetMessage<T>(out T? message) where T : class
        {
            try
            {
                message = _unwrap.Envelope<T>(_cloudEvent);
                return true;
            }
            catch (KeyNotFoundException)
            {
                message = null;
                return false;
            }
        }

        bool SerializerContext.TryGetMessage(Type messageType, out object? message) => 
            throw new NotImplementedException();

        IMessageSerializer SerializerContext.GetMessageSerializer() =>
            _factory.CreateSerializer();

        IMessageSerializer SerializerContext.GetMessageSerializer<T>(MessageEnvelope envelope, T message) =>
            _factory.CreateSerializer();

        IMessageSerializer SerializerContext.GetMessageSerializer(object message, string[] messageTypes) =>
            _factory.CreateSerializer();

        Dictionary<string, object> SerializerContext.ToDictionary<T>(T? message) where T : class => 
            throw new NotImplementedException();

        string[] SerializerContext.SupportedMessageTypes { get; } = Array.Empty<string>();
    }

    MessageBody IMessageDeserializer.GetMessageBody(string text) =>
        new StringMessageBody(text);

    public ContentType ContentType
    {
        get;
    }

    private class CloudEventContext : DeserializerConsumeContext
    {
        public CloudEventContext(ReceiveContext receiveContext, SerializerContext serializerContext) : base(receiveContext, serializerContext)
        {
        }

        public override bool HasMessageType(Type messageType) =>
            true;

        public override bool TryGetMessage<T>(out ConsumeContext<T>? consumeContext)
        {
            if (!SerializerContext.TryGetMessage<T>(out var message))
            {
                consumeContext = null;
                return false;
            }

            consumeContext = new MessageConsumeContext<T>(this, message!);
            return true;
        }

        public override Guid? MessageId => SerializerContext.MessageId;
        public override Guid? RequestId => SerializerContext.RequestId;
        public override Guid? CorrelationId => SerializerContext.CorrelationId;
        public override Guid? ConversationId => SerializerContext.ConversationId;
        public override Guid? InitiatorId => SerializerContext.InitiatorId;
        public override DateTime? ExpirationTime => SerializerContext.ExpirationTime;
        public override Uri? SourceAddress => SerializerContext.SourceAddress;
        public override Uri? DestinationAddress => SerializerContext.DestinationAddress;
        public override Uri? ResponseAddress => SerializerContext.ResponseAddress;
        public override Uri? FaultAddress => SerializerContext.FaultAddress;
        public override DateTime? SentTime => SerializerContext.SentTime;
        public override Headers Headers => SerializerContext.Headers;
        public override HostInfo Host => SerializerContext.Host;
        public override IEnumerable<string> SupportedMessageTypes => SerializerContext.SupportedMessageTypes;
    }
}