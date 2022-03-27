using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;

namespace StockBand.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _applicationDb;
        public AdminService(ApplicationDbContext applicationDb)
        {
            _applicationDb = applicationDb;
        }
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = await _applicationDb.UserDbContext.Include(x => x.Role).ToListAsync();
            if (users is null)
                return null;
            return users;
        }
    }
}
