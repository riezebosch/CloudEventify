using Rebus.Config;
using Rebus.Serialization.Custom;

namespace CloudEventify.Rebus;

public static class RebusConfigurerExtensions
{
    /// <summary>
    /// Use cloud events to wrap incoming and outgoing messages and be interoperable with the rest of the world.
    /// </summary>
    /// <param name="configurer"></param>
    /// <param name="options"></param>
    /// <returns>Use the <see cref="CustomTypeNameConventionBuilder"/> to map custom type names, either explicit with AddWithCustomName or implicit with AddWithShortName.</returns>
    public static RebusConfigurer UseCloudEvents(this RebusConfigurer configurer, Action<CloudEventsOptions>? options = null)
    {
        var factory = new CloudEventsOptions(configurer);
        options?.Invoke(factory);
        return factory.Configure();
    }
}