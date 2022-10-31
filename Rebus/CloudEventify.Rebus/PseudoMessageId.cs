using Rebus.Exceptions;
using Rebus.Messages;
using Rebus.Pipeline;

namespace CloudEventify.Rebus;

public class PseudoMessageId : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        var transportMessage = context.Load<TransportMessage>() ?? throw new RebusApplicationException("Could not find a transport message in the current incoming step context");
        if (!transportMessage.Headers.ContainsKey(Headers.MessageId))
        {
            var pseudoMessageId = GenerateMessageIdFromBodyContents(transportMessage.Body);
            transportMessage.Headers[Headers.MessageId] = pseudoMessageId;
        }

        await next();
    }

    /// <summary>
    /// Source: https://github.com/rebus-org/Rebus.RabbitMq/blob/44363284c11b97f63b89c4f2d928db9593275008/Rebus.RabbitMq/RabbitMq/RabbitMqTransport.cs#L580-L600 
    /// </summary>
    private static string GenerateMessageIdFromBodyContents(byte[]? body)
    {
        if (body == null) return "MESSAGE-BODY-IS-NULL";

        var base64String = Convert.ToBase64String(body);

        return $"knuth-{CalculateHash(base64String)}";
    }

    /// <summary>
    /// Source: https://github.com/rebus-org/Rebus.RabbitMq/blob/44363284c11b97f63b89c4f2d928db9593275008/Rebus.RabbitMq/Internals/Knuth.cs 
    /// </summary>
    private static ulong CalculateHash(string read)
    {
        var hashedValue = 3074457345618258791ul;
        for (var i = 0; i < read.Length; i++)
        {
            hashedValue += read[i];
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }
}