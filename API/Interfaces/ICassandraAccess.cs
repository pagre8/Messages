namespace API_Server.Interfaces
{
    public interface ICassandraAccess
    {
        Cassandra.ISession Session { get; }
    }
}
