namespace API_Server.Interfaces
{
    public interface IMessageService
    {
        Task<bool> CheckAccess(Guid userId, Guid chatId);
    }
}
