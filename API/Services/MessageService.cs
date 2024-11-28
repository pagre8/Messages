using API_Server.Interfaces;
using Serilog;

namespace API_Server.Services
{
    public class MessageService(ICassandraAccess cassandraAccess):IMessageService
    {
        private readonly ICassandraAccess _cassandraAccess = cassandraAccess;

        public async Task<bool> CheckAccess(Guid userId, Guid chatId)
        {
            try
            {
                var preparedCommand = await _cassandraAccess.Session.PrepareAsync("select count(*) from chat_participants where participant_id = ? and chat_id = ?");
                var boundCommand = preparedCommand.Bind(userId, chatId);

                var rs = await _cassandraAccess.Session.ExecuteAsync(boundCommand);

                return rs.FirstOrDefault() != null;
            }
            catch (Exception ex) { Log.Error($"Error checking access: {ex.Message} {ex.Source}"); }
            return false;

        }

    }
}
