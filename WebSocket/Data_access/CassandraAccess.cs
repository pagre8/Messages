﻿using Cassandra;

namespace WebSocket_Server.Data_access
{
    public class CassandraAccess : IDisposable
    {
        public readonly Cassandra.ISession _session;
        public CassandraAccess()
        {
            var cluster = Cluster.Builder()
                .AddContactPoint("localhost")
                .WithPort(9042)
                .WithCredentials("WebSocket", "WebSocket123")
                .Build();
            _session = cluster.Connect("messages_keyspace");
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }
}
