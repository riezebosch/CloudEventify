namespace CloudEventify.Dapr;

internal class CloudEvent<T> : global::Dapr.CloudEvent<T>
{
    // fixing dapr, since Id is a required property
    public string Id { get; } = Guid.NewGuid().ToString();

    public CloudEvent(T data) : base(data)
    {
    }
}