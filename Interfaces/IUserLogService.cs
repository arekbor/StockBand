using StockBand.Models;

namespace StockBand.Interfaces
{
    public interface IUserLogService
    {
        public Task<bool> AddToLogsAsync(string description, int id);
        public IQueryable<UserLog> GetAllUserLogs();
        public IQueryable<UserLog> GetAllLogs();
    }
}
