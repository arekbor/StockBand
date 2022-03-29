using Microsoft.AspNetCore.Mvc.ModelBinding;
using StockBand.Data;
using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Interfaces
{
    public interface IUserService
    {
        public Task<bool> LoginUserAsync(UserLoginDto userDto);
        public Task<bool> LogoutUserAsync();
        public Task<IEnumerable<User>> GetAllUsersAsync();
        public Task<User> GetUserAsync(int id);
        public Task<IEnumerable<Role>> GetAllRolesAsync();
        public Task<bool> UpdateUser(int id,EditUserDto model);
    }
}
