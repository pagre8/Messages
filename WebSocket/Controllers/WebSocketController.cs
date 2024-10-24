using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using WebSocket_Server.Infrastructure.Services;
using WebSocket_Server.Interfaces;

namespace WebSocket_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebSocketController(IMessageService messageService) : ControllerBase
    {
        private readonly IMessageService _messageService = messageService;

        [HttpGet("/ws")]
        public async Task<IActionResult> Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _messageService.HandleWebSocketCommunication(webSocket);
                return new EmptyResult();
            }
            else
            {
                return BadRequest("This endpoint only supports WebSocket requests.");
            }
        }
    }
}
