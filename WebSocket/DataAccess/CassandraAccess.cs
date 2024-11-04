using Cassandra;
using WebSocket_Server.Interfaces;

namespace WebSocket_Server.DataAccess
{
    public class CassandraAccess : IDisposable, ICassandraAccess
    {
        public virtual Cassandra.ISession Session { get; }
        public CassandraAccess(IConfiguration configuration)
        {
            var username = configuration["Cassandra:Username"];
            var password = configuration["Cassandra:Password"];

            var cluster = Cluster.Builder()
                .AddContactPoint("localhost")
                .WithPort(9042)
                .WithCredentials(username, password)
                .Build();
            Session = cluster.Connect("messages_keyspace");
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
