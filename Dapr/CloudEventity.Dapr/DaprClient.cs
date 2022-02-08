using System.Text.Json;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf;
using Grpc.Net.Client;
using DaprGrpcClient = Dapr.Client.Autogen.Grpc.v1.Dapr.DaprClient;

namespace CloudEventity.Dapr;

public class DaprClient
{
    private readonly string _address;

    private readonly GrpcChannelOptions _options = new()
    {
        // The gRPC client doesn't throw the right exception for cancellation
        // by default, this switches that behavior on.
        ThrowOperationCanceledOnCancellation = true,
    };

    public DaprClient(string address) => 
        _address = address;

    public async Task PublishEvent<TData>(string pubsub, string topic, TData message)
    {
        var data = new CloudEvent(CloudEventsSpecVersion.V1_0)
        {
            Id = Guid.NewGuid().ToString(),
            Source = new Uri("cloudeventify:dapr"),
            Data = message,
            Time = DateTimeOffset.Now,
            Type = typeof(TData).AssemblyQualifiedName
        };
        
        using var channel = GrpcChannel.ForAddress(_address, _options);
        var client = new DaprGrpcClient(channel);
        await client.PublishEventAsync(new PublishEventRequest
        {
            PubsubName = pubsub,
            Data = ByteString.CopyFrom(Format(data)),
            Topic = topic,
            DataContentType = "application/cloudevents+json"
        });
    }

    private static ReadOnlySpan<byte> Format(CloudEvent cloudEvent)
    {
        var formatter = new JsonEventFormatter(new JsonSerializerOptions(), new JsonDocumentOptions());
        return formatter.EncodeStructuredModeMessage(cloudEvent, out _).Span;
    }
}