using Rebus.Messages;
using Rebus.Pipeline;

namespace CloudEventify.Rebus;

public class RemoveRebusHeaders : IOutgoingStep
{
    public Task Process(OutgoingStepContext context, Func<Task> next)
    {
        var message = context.Load<TransportMessage>();
        message.Headers.Clear();

        return Task.CompletedTask;
    }
}