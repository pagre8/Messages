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
    public class MessageService(CassandraAccess cassandraAccess, RabbitAccess rabbitAccess) : IMessageService
    {
        private readonly CassandraAccess _cassandraAccess = cassandraAccess;
        private readonly RabbitAccess _rabbitAccess = rabbitAccess;

        public async Task HandleWebSocketCommunication(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var preparedCommand = await _cassandraAccess._session.PrepareAsync("Insert into messages (id, idchat, idsender, content, createdat) values (?, ?, ?, ?, ?)");

            try
            {
                using (var channel = _rabbitAccess.CreateModel())
                {
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
                                PublishToRabbit(channel, messageData);
                                SaveToCassandra(preparedCommand, messageData);
                            }
                            else
                            {
                                //send lack of access message

                                Log.Warning("No access");
                            }
                        }

                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }

                    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
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

        public void PublishToRabbit(IModel channel, MessageData messageData)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageData));
            channel.BasicPublish(
                exchange: "messages",
                routingKey: messageData.IdChat.ToString(),
                basicProperties: null,
                body: body
            );
        }

        public void SaveToCassandra(PreparedStatement preparedCommand, MessageData messageData)
        {
            try
            {
                var boundCommand = preparedCommand.Bind(
                    messageData.Id, messageData.IdChat, messageData.IdSender, messageData.Content, messageData.CreatedAt);
                _cassandraAccess._session.ExecuteAsync(boundCommand);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to Cassandra: {ex.Message}");
                Log.Error($"Error sending data to Cassandra: {ex.Message}");
            }
        }

        public async Task<bool> CheckAccess(Guid userId, Guid chatId)
        {
            try
            {
                var preparedCommand = await _cassandraAccess._session.PrepareAsync("select count(*) from chat_participants where participant_id = ? and chat_id = ?");
                var boundCommand = preparedCommand.Bind(userId, chatId);
                var rs = await _cassandraAccess._session.ExecuteAsync(boundCommand);

                var a = rs.Any();
                var b = rs.FirstOrDefault();
                var c = rs.Count();

                return rs.FirstOrDefault() != null;

            }
            catch (Exception ex) { Log.Error($"Error checking access: {ex.Message} {ex.Source}"); }
            return false;

        }
    }
}
