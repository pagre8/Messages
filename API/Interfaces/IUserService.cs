using API_Server.Models;

namespace API_Server.Interfaces
{
    public interface IUserService
    {
        Task<bool> CheckIfUserExistFromUsername(string username);
        Task<bool> CheckIfUserExistFromEmail(string email);
        Task<bool> RegisterUser(RegisterRequest registerRequest);

    }
}
