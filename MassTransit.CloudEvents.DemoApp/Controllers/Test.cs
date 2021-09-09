using System.Threading.Tasks;
using Dapr;
using Microsoft.AspNetCore.Mvc;

namespace MassTransit.CloudEvents.DemoApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Test : ControllerBase
    {
        [HttpPost]
        [Topic("my-pubsub", "user/loggedIn")]
        public async Task PostAsync(Data data, [FromServices]IUserLoggedIn handler) => 
            await handler.Handle(data.UserId);
        
        public record Data(int UserId);
    }
}