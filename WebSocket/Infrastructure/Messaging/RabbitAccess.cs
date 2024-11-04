using RabbitMQ.Client;
using WebSocket_Server.Interfaces;

namespace WebSocket_Server.Infrastructure.Messaging
{
    public class RabbitAccess : IDisposable, IRabbitAccess
    {
        public readonly IConnection _connection;

        public RabbitAccess()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
        }

        public IModel CreateModel()
        {
            return _connection.CreateModel();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }
}
