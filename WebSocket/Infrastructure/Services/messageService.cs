using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebSocket_Server.Data_access;
using WebSocket_Server.Models;
using WebSocket_Server.Infrastructure.Messaging;
using Cassandra;
using RabbitMQ.Client;

namespace WebSocket_Server.Services
{
    public class MessageService
    {
        private readonly CassandraAccess _cassandraAccess;
        private readonly RabbitAccess _rabbitAccess;

        public MessageService(CassandraAccess cassandraAccess, RabbitAccess rabbitAccess)
        {
            _cassandraAccess = cassandraAccess;
            _rabbitAccess = rabbitAccess;
        }

        public async Task HandleWebSocketCommunication(WebSocket webSocket)
        {
            Console.WriteLine("upgraded successfully");
            var buffer = new byte[1024 * 4];
            var preparedCommand =  await _cassandraAccess._session.PrepareAsync("Insert into messages (id, idchat, idsender, content, createdat) values (?, ?, ?, ?, ?)");

            var channel = _rabbitAccess._connection.CreateModel();
            channel.ExchangeDeclare(exchange: "messages", type: ExchangeType.Direct);

            try
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    var messageData = DeserializeMessage(receivedMessage);

                    bool hasAccess = await CheckAccess(messageData.IdSender, messageData.IdChat);

                    if (hasAccess)
                    {
                        if (messageData != null)
                        {
                            messageData.Id = Guid.NewGuid();
                            PublishToRabbit(channel, messageData);
                            SaveToCassandra(preparedCommand, messageData);
                        }
                    }
                    else
                    {
                        //send lack of access message
                    }
                    
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private MessageData DeserializeMessage(string message)
        {
            try
            {
                return JsonSerializer.Deserialize<MessageData>(message);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                return null;
            }
        }

        private void PublishToRabbit(IModel channel, MessageData messageData)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageData));
            channel.BasicPublish(
                exchange: "messages",
                routingKey: messageData.IdChat.ToString(),
                basicProperties: null,
                body: body
            );
            Console.WriteLine($" {DateTime.Now} Sent {messageData.Content}");
        }

        private void SaveToCassandra(PreparedStatement preparedCommand, MessageData messageData)
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
            }
        }

        private async Task<bool> CheckAccess(Guid userId, Guid chatId)
        {
            try
            {
                var preparedCommand = await _cassandraAccess._session.PrepareAsync("select count(*) from chat_participants where user_id = ? and chat_id = ?");
                var boundCommand = preparedCommand.Bind(userId, chatId);
                var rs = await _cassandraAccess._session.ExecuteAsync(boundCommand);
                if (rs != null && rs.FirstOrDefault() != null)
                {
                    var row = rs.First();
                    return row.GetValue<long>(0) > 0; //check if the count is greater than 0
                }
                return false;
            }
            catch (Exception ex) { Console.WriteLine($"Error checking access: {ex.Message}"); } 
            return false;

        }
    }
}
