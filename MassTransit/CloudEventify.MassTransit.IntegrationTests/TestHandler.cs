using System.Threading.Tasks;
using DaprApp.Controllers;
using Hypothesist;

namespace CloudEventify.MassTransit.IntegrationTests;

internal static class TestHandler
{
    public static IHandler<T> ToHandler<T>(this Observer<T> hypothesis) =>
        new Handler<T>(hypothesis);

    private class Handler<T>(Observer<T> observer) : IHandler<T>
    {
        public Task Handle(T data) => 
            observer.Add(data);
    }
}