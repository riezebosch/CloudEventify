using System.Threading.Tasks;
using DaprApp.Controllers;
using Hypothesist;

namespace MassTransit.CloudEvents.IntegrationTests
{
    internal static class Stubs
    {
        public static IConsumer<T> ToConsumer<T>(this IHypothesis<T> hypothesis) where T : class =>
            new Consumer<T>(hypothesis);
        
        public static IHandler<T> ToHandler<T>(this IHypothesis<T> hypothesis) =>
            new Handler<T>(hypothesis);

        private class Handler<T> : IHandler<T>
        {
            private readonly IHypothesis<T> _hypothesis;

            public Handler(IHypothesis<T> hypothesis) => 
                _hypothesis = hypothesis;

            public Task Handle(T data) => 
                _hypothesis.Test(data);
        }

        private class Consumer<T> : IConsumer<T> where T : class
        {
            private readonly IHypothesis<T> _hypothesis;

            public Consumer(IHypothesis<T> hypothesis) =>
                _hypothesis = hypothesis;

            public async Task Consume(ConsumeContext<T> context) =>
                await _hypothesis.Test(context.Message);
        }
    }

    
}