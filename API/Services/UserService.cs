using API_Server.DataAccess;
using API_Server.Interfaces;
using API_Server.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace API_Server.Services
{
    public class UserService(ApiDbContext _context): IUserService
    {

        public async Task<bool> CheckIfUserExistFromUsername(string username)
        {
            try
            {
                return await _context.UserData.AnyAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                Log.Error($"Error checking user existence: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckIfUserExistFromEmail(string email)
        {
            try
            {
                return await _context.UserData.AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                Log.Error($"Error checking user existence: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> RegisterUser(RegisterRequest registerRequest)
        {
            UserData userData = new()
            {
                CreatedAt = DateTime.UtcNow,
                Email = registerRequest.Email,
                Id = Guid.NewGuid(),
                Username = registerRequest.Username,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = "" // hash password
            };

            await _context.UserData.AddAsync(userData);

            try
            {
                await _context.SaveChangesAsync();
            } 
            catch (Exception ex)
            {
                Log.Error($"Error adding user: {ex.Message}");
                return false;
            }

            return true; //tmp
        }


    }
}
