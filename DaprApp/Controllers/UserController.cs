using System.Threading.Tasks;
using Dapr;
using Microsoft.AspNetCore.Mvc;

namespace DaprApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        [HttpPost]
        [Topic("my-pubsub", "user/loggedIn")]
        public async Task LoggedIn(Message message, [FromServices]IHandler<int> handler) => 
            await handler.Handle(message.UserId);
        
        public record Message(int UserId);
    }
}