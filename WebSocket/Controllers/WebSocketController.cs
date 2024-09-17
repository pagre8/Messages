using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using WebSocket_Server.Data_access;

namespace WebSocket_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebSocketController : ControllerBase
    {
        private readonly CassandraAccess _cassandraAccess;

        public WebSocketController(CassandraAccess cassandraAccess)
        {
            _cassandraAccess = cassandraAccess;
        }

        [HttpGet("/ws")]
        public async Task<IActionResult> Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await HandleWebSocketCommunication(webSocket);
                return new EmptyResult();
            }
            else
            {
                return BadRequest("This endpoint only supports WebSocket requests.");
            }
        }

        //TODO:
        //Make a propper Controller instead of test one

        private async Task HandleWebSocketCommunication(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while(!result.CloseStatus.HasValue)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Recived: {message}");
                var responseMessage = Encoding.UTF8.GetBytes($"Server response: {message}");
                await webSocket.SendAsync(new ArraySegment<byte>(responseMessage, 0, responseMessage.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer),CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

    }
}
