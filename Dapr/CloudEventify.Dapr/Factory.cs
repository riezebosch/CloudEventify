using Dapr.Client;

namespace CloudEventify.Dapr;

public static class Factory
{
    public static CloudEvents UseCloudEvents(this DaprClientBuilder builder) => 
        new Builder(builder);
}