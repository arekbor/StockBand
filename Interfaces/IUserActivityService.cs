using StockBand.Models;

namespace StockBand.Interfaces
{
    public interface IUserActivityService
    {
        public Task<bool> AddToActivityAsync(string description, int id);
        public IQueryable<UserActivity> GetAllUserActivityAsync(int id);
        public Task<IEnumerable<UserActivity>> GetAllActivityAsync();
    }
}
