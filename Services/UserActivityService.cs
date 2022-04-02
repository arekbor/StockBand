using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using System.Security.Claims;

namespace StockBand.Services
{
    public class UserActivityService : IUserActivityService
    {
        private readonly ApplicationDbContext _dbContext;
        public UserActivityService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> AddToActivityAsync(string description,int id)
        {
            if (String.IsNullOrEmpty(description))
                return false;
            var userActivity = new UserActivity()
            {
                Description = description,
                CreatedDate = DateTime.UtcNow,
                UserId = id
            };
            await _dbContext.UserActivityDbContext.AddAsync(userActivity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserActivity>> GetAllActivityAsync()
        {
            var usersActivities = await _dbContext
                .UserActivityDbContext
                .ToListAsync();
            if (usersActivities is null)
                return null;
            return usersActivities;
        }

        public IQueryable<UserActivity> GetAllUserActivityAsync(int id)
        {
            var userActivity = _dbContext
                .UserActivityDbContext
                .Where(x => x.UserId == id);
            return userActivity;
        }
    }
}
