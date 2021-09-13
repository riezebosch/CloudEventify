using System.Threading.Tasks;

namespace DaprApp.Controllers
{
    public interface IHandler<in T>
    {
        Task Handle(T data);
    }
}