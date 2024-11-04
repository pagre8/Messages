using Cassandra;
using WebSocket_Server.Interfaces;

namespace WebSocket_Server.DataAccess
{
    public class CassandraAccess : IDisposable, ICassandraAccess
    {
        public virtual Cassandra.ISession Session { get; }
        public CassandraAccess()
        {
            var cluster = Cluster.Builder()
                .AddContactPoint("localhost")
                .WithPort(9042)
                .WithCredentials("WebSocket", "WebSocket123")
                .Build();
            Session = cluster.Connect("messages_keyspace");
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
