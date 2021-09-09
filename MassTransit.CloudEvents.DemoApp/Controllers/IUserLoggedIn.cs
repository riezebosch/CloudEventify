using System.Threading.Tasks;

namespace MassTransit.CloudEvents.DemoApp.Controllers
{
    public interface IUserLoggedIn
    {
        Task Handle(int id);
    }
}