using System.Threading.Tasks;

namespace DaprApp.Controllers
{
    public interface IUserLoggedIn
    {
        Task Handle(int id);
    }
}