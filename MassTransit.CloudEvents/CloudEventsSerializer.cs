using System;
using System.IO;
using System.Net.Mime;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;

namespace MassTransit.CloudEvents
{
    public class CloudEventsSerializer : IMessageSerializer
    {
        public void Serialize<T>(Stream stream, SendContext<T> context) where T : class
        {
            var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
            {
                Data = context.Message,
                Source = new Uri("https://google.nl"),
                Id = context.MessageId.ToString(),
                Type = context.Message.GetType().Name
            };

            var formatter = new JsonEventFormatter();
            var message = formatter.EncodeStructuredModeMessage(cloudEvent, out _);
            stream.Write(message.Span);
        }

        public ContentType ContentType => new("application/cloudevents+json");
    }
}
