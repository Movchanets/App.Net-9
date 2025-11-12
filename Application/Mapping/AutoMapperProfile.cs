using Application.DTOs;
using AutoMapper;
using Infrastructure.Data.Models;
using Infrastructure.Entities;

namespace Application.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {

        CreateMap<UserEntity, UserVM>()
            .ForMember(
                dest => dest.UserRoles,
                opt => opt.MapFrom(src => src.UserRoles != null
                    ? src.UserRoles.Select(ur => ur.Role.Name).ToList()
                    : new List<string>()))
            .ForMember(
                dest => dest.Image,
                opt => opt.MapFrom(src => src.ImageUrl ?? string.Empty));

        CreateMap<UserEntity, UserDto>()
            // Map constructor parameters explicitly to ensure nulls become empty strings
            .ForCtorParam("Username", opt => opt.MapFrom(src => src.UserName ?? string.Empty))
            .ForCtorParam("Name", opt => opt.MapFrom(src => src.Name ?? string.Empty))
            .ForCtorParam("Surname", opt => opt.MapFrom(src => src.Surname ?? string.Empty))
            .ForCtorParam("Email", opt => opt.MapFrom(src => src.Email ?? string.Empty))
            .ForCtorParam("PhoneNumber", opt => opt.MapFrom(src => src.PhoneNumber ?? string.Empty))
            .ForCtorParam("Roles", opt => opt.MapFrom(src => src.UserRoles != null
                ? src.UserRoles.Select(ur => ur.Role != null ? ur.Role.Name : string.Empty).Where(n => !string.IsNullOrEmpty(n)).Select(n => n!).ToList()
                : new List<string>()));

        CreateMap<UserDto, UserEntity>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore());
    }
}