using AutoMapper;
using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Data
{
    public class ApplicationProfile:Profile
    {
        public ApplicationProfile()
        {
            //CreateMap<UserLoginDto, User>();
            CreateMap<User,EditUserDto>();
            //CreateMap<User,CreateUserDto>();
        }
    }
}
