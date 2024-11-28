using API_Server.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_Server.Controllers
{
    [Route("api/chats")]
    [ApiController]
    public class ChatsController(IMessageService messageService) : ControllerBase
    {
        private readonly IMessageService _messageService = messageService;

        //[HttpGet("{chatId}/messages")]
        //public async IActionResult GetMessages(string chatId, string userId, string cursor = null, int pageSize = 10)
        //{
        //    if (!Guid.TryParse(chatId, out _) || !Guid.TryParse(userId, out _))
        //    {
        //        return BadRequest("Invalid chatId or userId.");
        //    }

        //    if(!await _messageService.CheckAccess(Guid.Parse(userId),Guid.Parse(chatId)))
        //    {
        //        return Forbid();
        //    }
        //    //TODO
        //    // Finish controller
        //    return Ok("ok");

        //}
    }
}
