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
            var usersLogs = _dbContext
                .UserLogDbContext
                .AsQueryable();
            if (usersLogs is null)
                return null;
            return usersLogs;
        }
        public IQueryable<UserLog> GetAllUserLogsAsync()
        {
            var id = ParseUserId();
            var userLogs = _dbContext
                .UserLogDbContext
                .Where(x => x.UserId == id)
                .AsQueryable();
            if (userLogs is null)
                return null;
            return userLogs;
        }
        public async Task<bool> DeleteLogAsync(Guid id)
        {
            if (!GetUser().IsInRole(UserRoles.Roles[1]))
                return false;
            var log = await _dbContext
                .UserLogDbContext
                .FirstOrDefaultAsync(x => x.Guid == id);
            if (log is null)
                return false;
            _dbContext.UserLogDbContext.Remove(log);
            _dbContext.SaveChanges();
            return true;
        }
        private int ParseUserId()
        {
            return int.Parse(GetUser().FindFirst(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }
        private ClaimsPrincipal GetUser()
        {
            return _httpContextAccessor?.HttpContext?.User as ClaimsPrincipal;
        }
    }
}
