using System.Threading.Tasks;
using DaprApp.Controllers;
using Hypothesist;

namespace CloudEventify.Dapr.IntegrationTests;

internal static class TestHandler
{
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
}