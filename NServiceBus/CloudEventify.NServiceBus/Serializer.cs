using System.Text.Json;
using CloudNative.CloudEvents;
using NServiceBus.Logging;
using NServiceBus.Serialization;

namespace CloudEventify.NServiceBus;

/// <summary>
///     NServiceBus <see cref="IMessageSerializer" /> implementation for CloudEvents using the
///     <see cref="CloudEventFormatter" />
/// </summary>
public class Serializer : IMessageSerializer
{
    private readonly ILog _log;
    private readonly ITryMap? _map;
    private readonly JsonSerializerOptions _options;

    /// <summary>
    ///     Default constructor sets the JSonSerializerOptions to the CloudEvent standard with case insensitive property names
    ///     <see cref="JsonSerializerDefaults.Web" /> and adds the
    ///     <see cref="System.Text.Json.Serialization.JsonStringEnumConverter" /> to the options
    /// </summary>
    public Serializer(ITryMap map) : this(map, CloudEventExtensions.CloudEventStandardJsonSerializerOptions)
    {
    }

    private Serializer() : this(null, CloudEventExtensions.CloudEventStandardJsonSerializerOptions)
    {
    }

    /// <summary>
    ///     NServiceBus <see cref="IMessageSerializer" /> implementation for CloudEvents using the
    ///     <see cref="CloudEventFormatter" />
    /// </summary>
    /// <param name="map">a mapper containing type-name mapping for serialization / deserialization to the right .net type</param>
    /// <param name="options">
    ///     <see cref="JsonSerializerOptions" /> options to configure JSonSerializer used during
    ///     (de)serialization
    /// </param>
    public Serializer(ITryMap? map, JsonSerializerOptions options)
    {
        _log = LogManager.GetLogger<Serializer>();
        _map = map;
        _options = options;
    }

    /// <summary>
    ///     Sets the content type of the messages so they can be recognized by the deserializer
    /// </summary>
    public string ContentType => "application/cloudevents+json";

    /// <summary>
    ///     Serializes the given message for objects of type <see cref="CloudEvent{T}" /> or <see cref="CloudEvent" /> with the
    ///     configured <see cref="JsonSerializerOptions" /> using the <see cref="CloudEventFormatter" />
    /// </summary>
    /// <param name="message">Non null object of type <see cref="CloudEvent" /> or <see cref="CloudEvent{T}" /></param>
    /// <param name="stream">Initialized stream to write the the serialized CloudEvent too</param>
    public void Serialize(object message, Stream stream)
    {
        var eventTypeName = MapEventType(message);
        var cloudEvent = GetCloudEvent(message, eventTypeName);
        stream.Write(cloudEvent.ToByteArray(_options));
    }

    /// <summary>
    ///     Deserializes a serialized <see cref="CloudEvent{T}" /> using the <see cref="CloudEventFormatter" />
    ///     The CloudEvent.Type property must have a corresponding message type registered in the endpoint
    ///     <see cref="ICommand" /> and <see cref="IEvent" /> handlers
    /// </summary>
    /// <param name="body"></param>
    /// <param name="messageTypes"></param>
    /// <returns>array of deserialized objects</returns>
    /// <exception cref="ArgumentException">Could not find any type-mapping for {cloudEvent.Type}</exception>
    public object[] Deserialize(ReadOnlyMemory<byte> body, IList<Type> messageTypes)
    {
        var cloudEvent = body.ToArray().DecodeToCloudEvent(_options);
        var eventType = MapEventType(cloudEvent, messageTypes);
        _log.Info($"Deserializing CloudEvent Type {cloudEvent.Type} to {eventType.FullName}");
        var typedObject = GetTypedObject(eventType, cloudEvent);
        return typedObject != null ? new[] { typedObject } : Array.Empty<object>();
    }

    private static CloudEvent GetCloudEvent(object message, string? eventTypeName)
    {
        if (message is CloudEvent c)
        {
            c.Type = eventTypeName;
            return c;
        }

        var cloudEvent = message is WrappedCloudEvent wrappedCloudEvent
            ? wrappedCloudEvent.AsCloudEvent()
            : message as CloudEvent
              ?? CloudEventFactory.Create(message, null, null, eventTypeName, null);
        cloudEvent.Type = eventTypeName;

        return cloudEvent;
    }

    private string? MapEventType(object message)
    {
        _log.Info($"Mapping Event-Type from {message.GetType().FullName}");
        var eventType = message.GetType();
        if (message is CloudEvent cloudEvent)
        {
            if (cloudEvent.Type != null)
            {
                _log.Info($"Forced type of CloudEvent is {cloudEvent.Type}");
                return cloudEvent.Type;
            }

            eventType = cloudEvent.Data?.GetType();
        }

        _log.Info($"Derived type for mapping is {eventType?.FullName}");

        if (_map != null && eventType != null)
            if (_map.TryGet(eventType, out var mappedTypeName))
            {
                _log.Info("Mapped type name to " + mappedTypeName.Type + " for " + eventType.FullName);
                return mappedTypeName.Type;
            }

        _log.Info($"Unmapped type used as Event-Type: {eventType?.FullName}");
        return eventType?.FullName;
    }

    private Type MapEventType(CloudEvent cloudEvent, IList<Type> messageTypes)
    {
        var eventType = _map != null && cloudEvent.Type != null && _map.TryGet(cloudEvent.Type!, out var mappedType)
            ? mappedType
            : messageTypes.FirstOrDefault(t => t.FullName!.Equals(cloudEvent.Type));
        return eventType ?? throw new ArgumentException($"Could not find any type-mapping for {cloudEvent.Type}");
    }

    /// <summary>
    ///     Deserialize a <see cref="CloudEvent" /> to a <see cref="CloudEvent{T}" /> derived type or directly based on the
    ///     given <see cref="Type" /> type parameter
    /// </summary>
    /// <param name="type">
    ///     Either a type to which the Data property of the cloudEvent could be serialized or a CloudEvent{T}
    ///     derived type (<see cref="WrappedCloudEvent" />)
    /// </param>
    /// <param name="cloudEvent"></param>
    /// <returns>Deserialized object based on given Type (or null)</returns>
    private object? GetTypedObject(Type type, CloudEvent cloudEvent)
    {
        return type.IsSubclassOf(typeof(WrappedCloudEvent))
            ? Activator.CreateInstance(type, cloudEvent)!
            : type == typeof(CloudEvent)
                ? cloudEvent
                : cloudEvent.Data is JsonElement jsonElement
                    ? jsonElement.Deserialize(type, _options)!
                    : null;
    }
}