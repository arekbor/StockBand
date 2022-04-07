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
        public Task<bool> UpdateUser(int id,EditUserDto model);
        public Task<bool> CreateUser(Guid guid, CreateUserDto userDto);
        public Task<bool> ChangePasswordUser(ProfileEditUser userDto);
        public Task<bool> ChangeUserColor(SettingsUserDto userDto);
    }
}
