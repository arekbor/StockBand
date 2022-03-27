using StockBand.Models;

namespace StockBand.Interfaces
{
    public interface IAdminService
    {
        public Task<IEnumerable<User>> GetAllUsersAsync();
    }
}
