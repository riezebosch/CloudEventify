using System;
using System.Threading.Tasks;
using Dapr;
using Microsoft.AspNetCore.Mvc;

namespace DaprApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        [HttpPost(nameof(LoggedIn))]
        [Topic("my-pubsub", "user/loggedIn", "event.type ==\"loggedIn\"", 1)]
        public async Task LoggedIn(Message message, [FromServices]IHandler<int> handler) => 
            await handler.Handle(message.UserId);

        [HttpPost(nameof(Default))]
        [Topic("my-pubsub", "user/loggedIn")]
        public Task Default(Message message) =>
            throw new Exception("not expecting anything here");

        public record Message(int UserId);
    }
}