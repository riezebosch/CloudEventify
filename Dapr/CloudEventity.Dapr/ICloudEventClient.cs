namespace CloudEventity.Dapr;

public interface ICloudEventClient : IDisposable
{
    Task PublishEvent<TData>(string pubsub, string topic, TData message);
}