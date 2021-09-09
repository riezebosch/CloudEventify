using System.Net.Mime;

namespace MassTransit.CloudEvents
{
    public static class ConfiguratorExtensions
    {
        public static void UseCloudEvents(this IBusFactoryConfigurator cfg) =>
            cfg.UseCloudEventsFor(new ContentType("application/cloudevents+json"));
        
        public static void UseCloudEventsFor(this IBusFactoryConfigurator cfg, params ContentType[] contentTypes)
        {
            foreach (var contentType in contentTypes)
            {
                cfg.AddMessageDeserializer(contentType, () => new CloudEventsDeserializer { ContentType = contentType });    
            }

            cfg.SetMessageSerializer(() => new CloudEventsSerializer());
        }
    }
}