using RabbitMQ.Client;

namespace WebSocket_Server.Infrastructure.Messaging
{
    public class RabbitAccess : IDisposable
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
            _connection.Dispose();
        }

    }
}
