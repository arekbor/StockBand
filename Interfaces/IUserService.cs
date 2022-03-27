using Microsoft.AspNetCore.Mvc.ModelBinding;
using StockBand.Data;
using StockBand.ViewModel;

namespace StockBand.Interfaces
{
    public interface IUserService
    {
        public Task<bool> LoginUserAsync(UserLoginDto userDto);
        public Task<bool> LogoutUserAsync();
    }
}
