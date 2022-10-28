using Rebus.Messages;
using Rebus.Transport;
using System.Text.Json;

namespace CloudEventify.Rebus;

public class TransportDecorator : ITransport
{
    private ITransport _transport;

    public TransportDecorator(ITransport transport) => _transport = transport;

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

            var rbsHeaders = cloudEvent.GetRebusHeaders();
            foreach (var rbsHeader in rbsHeaders)
            {
                message.Headers[rbsHeader.Key] = rbsHeader.Value;
            }

            //Must meet property..
            message.Headers[Headers.MessageId] = cloudEvent.Id!;
            message.Headers[Headers.SentTime] = cloudEvent.Time?.ToString("O")!;
        }
        return message;
    }


    public async Task Send(string destinationAddress, TransportMessage message, ITransactionContext context) => await _transport.Send(destinationAddress, message, context);
}