namespace CloudEventify.Dapr;

public interface ICloudEventClientBuilder : IConfigure<ICloudEventClientBuilder>
{
    ICloudEventClient Build();
}