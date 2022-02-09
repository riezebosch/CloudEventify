using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rebus.Activation;
using Rebus.Handlers;
using Rebus.Transport;

namespace CloudEventify.Rebus.IntegrationTests;

public class EmptyActivator : IHandlerActivator
{
    Task<IEnumerable<IHandleMessages<TMessage>>> IHandlerActivator.GetHandlers<TMessage>(TMessage message, ITransactionContext transactionContext) => 
        Task.FromResult(Enumerable.Empty<IHandleMessages<TMessage>>());
}