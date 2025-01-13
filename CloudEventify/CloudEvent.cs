using System.Reflection;
using System.Text.Json;
using CloudNative.CloudEvents;

namespace CloudEventify;

/// <summary>
/// A CloudEvent is a data structure that describes an event. This wraps the Native <see cref="CloudEvent"/> and makes the Data point Typed.
/// To use this class, inherit from it and pass the data to the base constructor. For compatiblity with the serializer implement the constructor that takes a <see cref="CloudEvent"/> and pass it to the base constructor.
/// Default conventions (of nothing is overriden) are:
/// Time = DateTimeOffset.UtcNow
/// Subject = $"{typeof(T).Name}/{GetType().Name}"
/// Source = new Uri($"app://{EntryAssembly.FullName}")
/// DataContentType = "application/json"
/// Type = GetType().FullName 
/// </summary>
/// <typeparam name="T">Any data type/record that needs to be packaged as part of the CloudEvent</typeparam>
public class CloudEvent<T> : WrappedCloudEvent {

    public T? Data => (T?)CloudEvent.Data;

    /// <summary>
    /// This constructor is most commonly used to create new events with a Typed payload, where default conventions are used for CloudEvent properties.
    /// </summary>
    /// <param name="data">Typed data payload of the event</param>
    public CloudEvent(T data) : this(data, null, null, null, null)
    {
    }
    
    /// <summary>
    /// This constructor is most commonly used to create new events with a Typed payload, where you can override a number of CloudEvent properties to your own conventions.
    /// </summary>
    /// <param name="data">Typed data payload of the event</param>
    /// <param name="subject"></param>
    /// <param name="typeName"></param>
    /// <param name="dataSchema"></param>
    /// <param name="source"></param>
    public CloudEvent(T data, string? subject, string? typeName, Uri? dataSchema, Uri? source)
    {
        CloudEvent = CloudEventFactory.Create(data, subject ?? $"{typeof(T).Name!}/{GetType().Name}",
            source ?? new Uri($"app://{Assembly.GetEntryAssembly()?.FullName!.Split(',')[0]}"),
            typeName ?? GetType().FullName!, dataSchema);
    }

    public CloudEvent(byte[] cloudEventData): this(cloudEventData, CloudEventExtensions.CloudEventStandardJsonSerializerOptions) { }
    public CloudEvent(byte[] cloudEventData, JsonSerializerOptions options) : this(cloudEventData.DecodeToCloudEvent(options), options) { }

    public CloudEvent(CloudEvent cloudEvent) : this(cloudEvent, CloudEventExtensions.CloudEventStandardJsonSerializerOptions) { }
    public CloudEvent(CloudEvent cloudEvent, JsonSerializerOptions options)
    {
        if (cloudEvent.Data is JsonElement jsonElement)
        {
            cloudEvent.Data = jsonElement.Deserialize<T>(options);
        }
        CloudEvent = cloudEvent.Data is T
            ? cloudEvent
            : throw new ArgumentException(
                $"CloudNative object does not contain a Data element that is or can be converted to {typeof(T).FullName}",
                nameof(cloudEvent));
    }

}

public static class CloudEventFactory {
    public static CloudEvent Create(object? data, string? subject, Uri? source, string? typeName, Uri? dataSchema) 
    {
        return new CloudEvent(CloudEventsSpecVersion.V1_0)
        {
            Id = Guid.NewGuid().ToString(),
            Time = DateTimeOffset.UtcNow,
            Subject = subject ?? data?.GetType().Name,
            Source = source ?? new Uri($"app://{Assembly.GetEntryAssembly()?.FullName!.Split(',')[0]}"),
            DataContentType = "application/json",
            Data = data,
            Type = typeName ?? data?.GetType().FullName!,
            DataSchema = dataSchema,
        };
    }
}