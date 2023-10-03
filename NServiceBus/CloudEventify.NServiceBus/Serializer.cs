using System.Text.Json;
using CloudNative.CloudEvents;
using NServiceBus.MessageInterfaces;
using NServiceBus.Serialization;
using NServiceBus.Settings;

namespace CloudEventify.NServiceBus;

/// <summary>
/// NServiceBus <see cref="IMessageSerializer"/> implementation for CloudEvents using the <see cref="CloudEventFormatter"/>
/// </summary>
/// <param name="options"></param>
public class Serializer(JsonSerializerOptions options) : SerializationDefinition, IMessageSerializer
{
    /// <summary>
    /// Sets the content type of the messages so they can be recognized by the deserializer
    /// </summary>
    public string ContentType => "application/cloudevents+json";
    
    /// <summary>
    /// Implement construction of the CloudEvent Message Serializer interface required by the SerializationDefinition
    /// </summary>
    /// <param name="settings"></param>
    /// <returns><see cref="Serializer"/></returns>
    public override Func<IMessageMapper, IMessageSerializer> Configure(IReadOnlySettings settings) => _ => this;
    
    /// <summary>
    /// Default constructor sets the JSonSerializerOptions to the CloudEvent standard with case insensitive property names <see cref="JsonSerializerDefaults.Web"/> and adds the <see cref="System.Text.Json.Serialization.JsonStringEnumConverter"/> to the options
    /// </summary>
    public Serializer():this(CloudEventExtensions.CloudEventStandardJsonSerializerOptions){}

    /// <summary>
    /// Serializes the given message for objects of type <see cref="CloudEvent{T}"/> or <see cref="CloudEvent"/> with the configured <see cref="JsonSerializerOptions"/> using the <see cref="CloudEventFormatter"/> 
    /// </summary>
    /// <param name="message">Non null object of type <see cref="CloudEvent"/> or <see cref="CloudEvent{T}"/></param>
    /// <param name="stream">Initialized stream to write the the serialized CloudEvent too</param>
    public void Serialize(object message, Stream stream)
    {
        var data = message is WrappedCloudEvent wrappedCloudEvent
            ? wrappedCloudEvent.ToByteArray(options)
            : message is CloudEvent cloudEvent
                ? cloudEvent.ToByteArray(options)
                : CloudEventFactory.Create(message,null,null,null,null).ToByteArray(options); 
        stream.Write(data);
    }

    /// <summary>
    /// Deserializes a serialized <see cref="CloudEvent{T}"/> using the <see cref="CloudEventFormatter"/>
    /// The CloudEvent.Type property must have a corresponding message type registered in the endpoint <see cref="ICommand"/> and <see cref="IEvent"/> handlers
    /// </summary>
    /// <param name="body"></param>
    /// <param name="messageTypes"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public object[] Deserialize(ReadOnlyMemory<byte> body, IList<Type> messageTypes)
    {
        var cloudEvent =  body.ToArray().DecodeToCloudEvent(options);
        var eventType = messageTypes.FirstOrDefault(t => t.FullName!.Equals(cloudEvent.Type));
        return new[] { GetTypedObject(eventType, cloudEvent) };
    }
    
    /// <summary>
    /// Deserialize a <see cref="CloudEvent"/> to a <see cref="CloudEvent{T}"/> derived type or directly based on the given <see cref="Type"/> type parameter
    /// </summary>
    /// <param name="type">Either a type to which the Data property of the cloudEvent could be serialized or a CloudEvent{T} derived type (<see cref="WrappedCloudEvent"/>)</param>
    /// <param name="cloudEvent"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public object GetTypedObject(Type? type, CloudEvent cloudEvent)
    {
        if (type is null)
            throw new ArgumentException($"Could not find handler for type {cloudEvent.Type}, cannot process message after deserialization", nameof(type));
        if (cloudEvent.Data is null)
            throw new ArgumentException($"CloudEvent.Data is null, cannot process message after deserialization", nameof(cloudEvent));
                
        return (type.IsSubclassOf(typeof(WrappedCloudEvent))) 
            ? Activator.CreateInstance(type, cloudEvent)!
            : (cloudEvent.Data!=null && cloudEvent.Data is JsonElement jsonElement)
                ? jsonElement.Deserialize(type, options)!
                : throw new ArgumentException($"Unable to deserialize data of the CloudEvent {cloudEvent.Data} to {type.FullName}", nameof(cloudEvent));
        
    }

}