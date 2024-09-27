using RabbitMQ.Client;

namespace WebSocket_Server.Rabbit_Access
{
    public class RabbitAccess : IDisposable
    {
        public readonly IConnection _connection;
        
        public RabbitAccess()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
        }





        public void Dispose() 
        {
            _connection.Dispose();
        }

    }
}
