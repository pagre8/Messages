using Cassandra;

namespace WebSocket_Server.Data_access
{
    public class CassandraAccess
    {
        private readonly Cassandra.ISession _session;
        public CassandraAccess()
        {
            var cluster = Cluster.Builder()
                .AddContactPoint("localhost")
                .WithPort(9042)
                .WithCredentials("cassandra", "cassandra")
                .Build();
            //TODO: Create a proper keyspace
            _session = cluster.Connect("tmp_keyspace");
        }
    }
}
