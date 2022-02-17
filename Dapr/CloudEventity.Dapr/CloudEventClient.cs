using CloudEventify;
using CloudNative.CloudEvents;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf;
using Grpc.Net.Client;

namespace CloudEventity.Dapr;

internal sealed class CloudEventClient : ICloudEventClient
{
    private readonly GrpcChannel _channel;
    private readonly CloudEventFormatter _formatter;
    private readonly Wrap _wrap;

    public CloudEventClient(GrpcChannel channel, CloudEventFormatter formatter, Wrap wrap)
    {
        _channel = channel;
        _formatter = formatter;
        _wrap = wrap;
    }


    public async Task PublishEvent<TData>(string pubsub, string topic, TData message)
    {
        var client = new global::Dapr.Client.Autogen.Grpc.v1.Dapr.DaprClient(_channel);
        await client.PublishEventAsync(new PublishEventRequest
        {
            PubsubName = pubsub,
            Data = ByteString.CopyFrom(_formatter.Encode(_wrap.Envelope(message!))),
            Topic = topic,
            DataContentType = "application/cloudevents+json"
        });
    }

    void IDisposable.Dispose() => 
        _channel.Dispose();
}