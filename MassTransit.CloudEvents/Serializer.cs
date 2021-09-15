using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using CloudNative.CloudEvents;

namespace MassTransit.CloudEvents
{
    public class Serializer : IMessageSerializer
    {
        private readonly Dictionary<Type, string> _types = new();

        public void Serialize<T>(Stream stream, SendContext<T> context) where T : class
        {
            var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
            {
                Data = context.Message,
                Source = context.SourceAddress ?? new Uri("cloudeventify:masstransit"),
                Id = context.MessageId.ToString(),
                Type = Type(context.Message.GetType()),
                Time = GetTimeValue?.Invoke()
            };

            stream.Write(cloudEvent.ToMessage().Span);
        }

        private Func<DateTimeOffset?> GetTimeValue;
        public void ConfigureTimeAttribute(Func<DateTimeOffset?> getTime)
        {
            GetTimeValue = getTime;
        }

        public ContentType ContentType
        {
            get;
            set;
        } = new("application/cloudevents+json");

        public void AddType<T>(string type) =>
            _types[typeof(T)] = type;
    
        private string Type(Type type) => 
            _types.TryGetValue(type, out var result) ? result : type.Name;
    }
}
