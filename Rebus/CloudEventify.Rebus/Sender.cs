using Rebus.Messages;
using Rebus.Pipeline;

namespace CloudEventify.Rebus;

public class Sender : IOutgoingStep
{
    private readonly string _address;

    public Sender(string address) => _address = address;

    public async Task Process(OutgoingStepContext context, Func<Task> next)
    {
        var message = context.Load<Message>();
        message.Headers.TryAdd(Headers.SenderAddress, _address);

        await next();
    }
}