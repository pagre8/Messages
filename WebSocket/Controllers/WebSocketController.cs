using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using WebSocket_Server.Services;
using WebSocket_Server.Models;

namespace WebSocket_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebSocketController : ControllerBase
    {
        private readonly MessageService _messageService;

        public WebSocketController(MessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet("/ws")]
        public async Task<IActionResult> Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                Console.WriteLine("incoming connection");
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
