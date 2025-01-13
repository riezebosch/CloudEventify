using System.Text;
using System.Text.Json;
using CloudNative.CloudEvents;

namespace CloudEventify;

public abstract class WrappedCloudEvent
{
    protected CloudEvent CloudEvent = new(CloudEventsSpecVersion.V1_0);
    public CloudEvent AsCloudEvent() => CloudEvent;
    
    public string? Id => CloudEvent.Id;
    public Uri? Source => CloudEvent.Source;

    public string? Subject
    {
        get => CloudEvent.Subject;
        set => CloudEvent.Subject = value;
    }

    public string? DataContentType => CloudEvent.DataContentType;
    public Uri? DataSchema => CloudEvent.DataSchema;
    public DateTimeOffset? Time => CloudEvent.Time;
    public string? Type => CloudEvent.Type;
    public bool IsValid => CloudEvent.IsValid;
    
    public byte[] ToByteArray(JsonSerializerOptions options) => CloudEvent.ToByteArray(options);

    public string ToString(JsonSerializerOptions options) => Encoding.UTF8.GetString(CloudEvent.ToByteArray(options));
    public override string ToString() => ToString(CloudEventExtensions.CloudEventStandardJsonSerializerOptions);
}