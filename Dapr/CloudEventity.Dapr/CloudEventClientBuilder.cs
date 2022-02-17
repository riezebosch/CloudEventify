using System.Text.Json;
using CloudEventify;
using Grpc.Net.Client;

namespace CloudEventity.Dapr;

public class CloudEventClientBuilder : ICloudEventClientBuilder
{
    private readonly string _address;
    
    private readonly GrpcChannelOptions _grpc = new()
    {
        // The gRPC client doesn't throw the right exception for cancellation
        // by default, this switches that behavior on.
        ThrowOperationCanceledOnCancellation = true,
    };

    private readonly ITypesMap _mapper = new TypesMapper();
    private readonly JsonSerializerOptions _json = new();

    private CloudEventClientBuilder(string address) => 
        _address = address;

    ICloudEventClientBuilder IConfigure<ICloudEventClientBuilder>.WithJsonOptions(Action<JsonSerializerOptions> options)
    {
        options(_json);
        return this;
    }

    ICloudEventClientBuilder IConfigure<ICloudEventClientBuilder>.WithTypes(Func<ITypesMap, ITypesMap> map)
    {
        map(_mapper);
        return this;
    }

    ICloudEventClient ICloudEventClientBuilder.Build() =>
        new CloudEventClient( GrpcChannel.ForAddress(_address, _grpc), Formatter.New(_json), new Wrap(_mapper));

    public static ICloudEventClientBuilder For(string address) => 
        new CloudEventClientBuilder(address);
}