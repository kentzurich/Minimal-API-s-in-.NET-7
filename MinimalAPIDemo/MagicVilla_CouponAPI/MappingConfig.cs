﻿using AutoMapper;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;

namespace MagicVilla_CouponAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig() 
        {
            CreateMap<Coupon, CouponCreateDto>().ReverseMap();
            CreateMap<Coupon, CouponUpdateDto>().ReverseMap();
            CreateMap<Coupon, CouponDTO>().ReverseMap();

            CreateMap<LocalUser, RegistrationRequestDto>().ReverseMap();
            CreateMap<LocalUser, UserDto>().ReverseMap();
        }
    }
}
