using Rebus.Messages;
using Rebus.Transport;
using System.Text.Json;

namespace CloudEventify.Rebus;

public class CloudEventAttributeHeaderDecorator : ITransport
{
    private ITransport _transport;
    private RebusHeader2CloudAttributeMap _mapper;

    public CloudEventAttributeHeaderDecorator(ITransport transport)
    {
        _transport = transport;
        _mapper = new RebusHeader2CloudAttributeMap();
    }

    public string Address => _transport.Address;

    public void CreateQueue(string address) => _transport.CreateQueue(address);

    public async Task<TransportMessage?> Receive(ITransactionContext context, CancellationToken cancellationToken)
    {
        var message = await _transport.Receive(context, cancellationToken);
        if (message != null)
        {
            var cloudEvent = Formatter
                                    .New(new JsonSerializerOptions(JsonSerializerDefaults.General))
                                    .DecodeStructuredModeMessage(message.Body, null, null);

            //Must meet property..
            message.Headers[Headers.MessageId] = cloudEvent.Id;

            //Other headers that may be populated if rebus is the publisher/sender..
            foreach (var rbsCloudeAttribName in _mapper.Rebus2CloudMap.Values)
            {
                var attrib = cloudEvent.ExtensionAttributes.FirstOrDefault(ea => ea.Name.Equals(rbsCloudeAttribName));
                if (attrib != null)
                {
                    var value = cloudEvent[attrib];
                    if (value != null)
                    {
                        message.Headers[_mapper.FromCloudAttribute(rbsCloudeAttribName)] = (string)value;
                    }
                }

            }
        }
        return message;
    }
    
    public async Task Send(string destinationAddress, TransportMessage message, ITransactionContext context) => await _transport.Send(destinationAddress, message, context);
}