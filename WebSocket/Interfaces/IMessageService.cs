using Cassandra;
using RabbitMQ.Client;
using System.Net.WebSockets;
using WebSocket_Server.Models;

namespace WebSocket_Server.Interfaces
{
    public interface IMessageService
    {
        Task HandleWebSocketCommunication(WebSocket webSocket);

        Task<bool> SaveToCassandra(PreparedStatement preparedCommand, MessageData messageData);
        bool PublishToRabbit(IModel channel, MessageData messageData);
        Task<bool> CheckAccess(Guid userId, Guid chatId);

        Task SendMessageAsync(WebSocket webSocket, string message);
    }
}
