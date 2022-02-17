using CloudEventify;

namespace CloudEventity.Dapr;

public interface ICloudEventClientBuilder : IConfigure<ICloudEventClientBuilder>
{
    ICloudEventClient Build();
}