using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data_access
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }
        public DbSet<UserData> User_data => Set<UserData>();
        public DbSet<Messages> Messages => Set<Messages>();
    }
}
