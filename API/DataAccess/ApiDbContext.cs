using API_Server.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Server.DataAccess
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }
        public DbSet<UserData> UserData => Set<UserData>();
    }
}
