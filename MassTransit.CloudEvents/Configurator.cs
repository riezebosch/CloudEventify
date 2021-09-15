using System;
using System.Net.Mime;

namespace MassTransit.CloudEvents
{
    internal class Configurator : IConfigurator
    {
        private readonly Serializer _serializer;
        private readonly Deserializer _deserializer;

        public Configurator(Serializer serializer, Deserializer deserializer)
        {
            _serializer = serializer;
            _deserializer = deserializer;
        }
        
        public IConfigurator WithContentType(ContentType contentType)
        {
            _serializer.ContentType =
                _deserializer.ContentType = contentType;
            
            return this;
        }

        public IConfigurator Type<T>(string type)
        {
            _deserializer.AddType<T>(type);
            _serializer.AddType<T>(type);
            
            return this;
        }

        public IConfigurator WithTimeAttributeValueOnSerialize(Func<DateTimeOffset?> getTime)
        {
            _serializer.ConfigureTimeAttribute(getTime);
            return this;
        }
    }
}