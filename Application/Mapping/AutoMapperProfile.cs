using Application.DTOs;
using AutoMapper;
using Infrastructure.Entities;

namespace Application.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<UserEntity, UserDto>().ReverseMap();
    }
}