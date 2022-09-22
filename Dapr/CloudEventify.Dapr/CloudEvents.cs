using Dapr.Client;

namespace CloudEventify.Dapr;

public interface CloudEvents : Types<CloudEvents>
{
    DaprClient Build();
}