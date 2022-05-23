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
        public IQueryable<User> GetAllUsersAsync();
        public Task<User> GetUserAsync(int id);
        public Task<bool> UpdateUser(int id, SettingsUserDto model);
        public Task<bool> CreateUser(Guid guid, CreateUserDto userDto);
        public Task<bool> ChangePasswordUser(ChangePasswordDto userDto);
        public Task<bool> ChangeUserColor(ChangeColorDto userDto);
        public Task<bool> ChangeUserTheme(ChangeThemeDto userDto);
        public Task<bool> RemoveUserCookie();
        public Task<bool> UpdateRememberMeStatus(int id, bool rememberMe);
        public Task<User> GetUserByName(string name);
        public Task<bool> UpdateUserImages(EditUserDto userDto, UserProfileImagesTypes type);
        public Task<bool> RemoveUserImage(UserProfileImagesTypes type);
    }
}
