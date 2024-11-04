using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebSocket_Server.DataAccess;
using WebSocket_Server.Models;
using WebSocket_Server.Infrastructure.Messaging;
using Cassandra;
using RabbitMQ.Client;
using Serilog;
using WebSocket_Server.Interfaces;

namespace WebSocket_Server.Infrastructure.Services
{
    public class MessageService(ICassandraAccess cassandraAccess, IRabbitAccess rabbitAccess) : IMessageService
    {
        private readonly ICassandraAccess _cassandraAccess = cassandraAccess;
        private readonly IRabbitAccess _rabbitAccess = rabbitAccess;

        public async Task HandleWebSocketCommunication(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var preparedCommand = await _cassandraAccess.Session.PrepareAsync("Insert into messages (id, idchat, idsender, content, createdat) values (?, ?, ?, ?, ?)");

            try
            {
                using var channel = _rabbitAccess.CreateModel();
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    var messageData = DeserializeMessage(receivedMessage);

                    if (messageData != null)
                    {
                        bool hasAccess = await CheckAccess(messageData.IdSender, messageData.IdChat);

                        if (hasAccess)
                        {
                            messageData.Id = Guid.NewGuid();
                            
                            if(await SaveToCassandra(preparedCommand, messageData))
                            {
                                PublishToRabbit(channel, messageData);
                            }
                            else
                            {
                                await SendMessageAsync(webSocket, "Internal server error.");
                            }
                        }
                        else
                        {
                            await SendMessageAsync(webSocket, "You don't have access to this chat.");

                            Log.Warning("No access");
                        }
                    }

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        static private MessageData? DeserializeMessage(string message)
        {
            try
            {
                return JsonSerializer.Deserialize<MessageData>(message);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                Log.Error($"Error deserializing JSON: {ex.Message}");
                return null;
            }
        }

        public bool PublishToRabbit(IModel channel, MessageData messageData)
        {

            if (!IsValidMessageData(messageData))
            {
                throw new ArgumentException("Invalid message data.");
            }
            try
            {
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageData));
                channel.BasicPublish(
                    exchange: "messages",
                    routingKey: messageData.IdChat.ToString(),
                    basicProperties: null,
                    body: body
                );
                return true;
            }
            catch (Exception ex) 
            {
                Log.Error("Error publishing to Rabbit. "+ex.Message);
                return false;
            }
        }

        public async Task<bool> SaveToCassandra(PreparedStatement preparedCommand, MessageData messageData)
        {
            if (!IsValidMessageData(messageData))
            {
                throw new ArgumentException("Invalid message data.");
            }

            try
            {
                var boundCommand = preparedCommand.Bind(
                    messageData.Id, messageData.IdChat, messageData.IdSender, messageData.Content, messageData.CreatedAt);
                await _cassandraAccess.Session.ExecuteAsync(boundCommand);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Error sending data to Cassandra: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckAccess(Guid userId, Guid chatId)
        {
            try
            {
                var preparedCommand = await _cassandraAccess.Session.PrepareAsync("select count(*) from chat_participants where participant_id = ? and chat_id = ?");
                var boundCommand = preparedCommand.Bind(userId, chatId);

                var rs = await _cassandraAccess.Session.ExecuteAsync(boundCommand);

                return rs.FirstOrDefault() != null;
            }
            catch (Exception ex) { Log.Error($"Error checking access: {ex.Message} {ex.Source}"); }
            return false;

        }

        private static bool IsValidMessageData(MessageData messageData)
        {
            if (messageData == null) return false;
            if (messageData.Id == Guid.Empty) return false;
            if (messageData.IdChat == Guid.Empty) return false;
            if (messageData.IdSender == Guid.Empty) return false;
            if(string.IsNullOrEmpty(messageData.Content)) return false;
            if (messageData.CreatedAt > DateTime.UtcNow) return false;

            return true;
        }

        public async Task SendMessageAsync(WebSocket webSocket, string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var body = new ArraySegment<byte>(buffer);
            try
            {
                await webSocket.SendAsync(body, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
            }catch(Exception ex)
            {
                Log.Error("Error sending message back to user. " + ex.Message);
            }
        }
    }
}
