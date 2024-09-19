using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using WebSocket_Server.Data_access;
using System.Text.Json;
using WebSocket_Server.Models;

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
            var preparedCommand = _cassandraAccess._session.Prepare("Insert into messages (id, idchat, idsender, content, createdat) values (?, ?, ?, ?, ?)");
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while(!result.CloseStatus.HasValue)
            {
                var recivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                try
                {
                    MessageData messageData = JsonSerializer.Deserialize<MessageData>(recivedMessage);
                    try
                    {
                        var boundCommand = preparedCommand.Bind(messageData.Id, messageData.IdChat, messageData.IdSender, messageData.Content, messageData.CreatedAt);
                        _cassandraAccess._session.Execute(boundCommand);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending data to cassandra: {ex.Message}");
                    }

                }
                catch (JsonException ex) 
                {
                    Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                }
                


                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer),CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

    }
}
