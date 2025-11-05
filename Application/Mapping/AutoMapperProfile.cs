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
        // For AutoMapper v15 constructor mapping behavior may differ; explicitly construct UserDto using constructor
        .ConstructUsing(src => new UserDto(
            src.UserName ?? string.Empty,
            src.Name ?? string.Empty,
            src.Surname ?? string.Empty,
            src.Email ?? string.Empty,
            src.UserRoles != null
                ? src.UserRoles.Select(ur => ur.Role != null ? ur.Role.Name : string.Empty).Where(n => !string.IsNullOrEmpty(n)).Select(n => n!).ToList()
                : new List<string>()));

        CreateMap<UserDto, UserEntity>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore());
    }
}