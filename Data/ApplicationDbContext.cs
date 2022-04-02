using Microsoft.EntityFrameworkCore;
using StockBand.Models;

namespace StockBand.Data
{
    public class ApplicationDbContext:DbContext
    {
        public DbSet<User> UserDbContext { get; set; }
        public DbSet<UserActivity> UserActivityDbContext { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> contextOptions) : base(contextOptions)
        {

        }
    }
}
