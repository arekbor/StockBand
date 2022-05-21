﻿using AutoMapper;
using StockBand.Models;
using StockBand.ViewModel;

namespace StockBand.Data
{
    public class ApplicationProfile:Profile
    {
        public ApplicationProfile()
        {
            CreateMap<User, SettingsUserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<Link, UniqueLinkMinutesDto>();
            CreateMap<AddTrackDto, Track>();
            CreateMap<User, UserDto> ();
        }
    }
}
