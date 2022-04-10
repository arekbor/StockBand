using Microsoft.EntityFrameworkCore;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;
using System.Security.Claims;

namespace StockBand.Services
{
    public class UserLogService : IUserLogService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserLogService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<bool> AddToLogsAsync(string description, int id)
        {
            if (String.IsNullOrEmpty(description))
                return false;
            var userActivity = new UserLog()
            {
                Action = description,
                CreatedDate = DateTime.Now,
                UserId = id
            };
            
            await _dbContext.UserLogDbContext.AddAsync(userActivity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public IQueryable<UserLog> GetAllLogsAsync()
        {
            var usersActivities = _dbContext
                .UserLogDbContext
                .AsQueryable();
            if (usersActivities is null)
                return null;
            return usersActivities;
        }
        public IQueryable<UserLog> GetAllUserLogsAsync()
        {
            var id = int.Parse(GetUser().FindFirst(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var userActivity = _dbContext
                .UserLogDbContext
                .Where(x => x.UserId == id);
            return userActivity;
        }
        private ClaimsPrincipal GetUser()
        {
            return _httpContextAccessor?.HttpContext?.User as ClaimsPrincipal;
        }
    }
}
