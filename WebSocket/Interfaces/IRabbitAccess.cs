using RabbitMQ.Client;

namespace WebSocket_Server.Interfaces
{
    public interface IRabbitAccess
    {
        IModel CreateModel();
    }
}
