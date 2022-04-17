using StockBand.Models;

namespace StockBand.Interfaces
{
    public interface IUserLogService
    {
        public Task<bool> AddToLogsAsync(string description, int id);
        public IQueryable<UserLog> GetAllUserLogsAsync();
        public IQueryable<UserLog> GetAllLogsAsync();
        public Task<bool> DeleteLogAsync(Guid id);
    }
}
