using Application.DTOs;
using AutoMapper;
using Application.Models;
using Domain.Entities;

namespace Application.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Map from Domain User to UserVM (identity-specific fields left default)
        CreateMap<User, UserVM>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdentityUserId))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name ?? string.Empty))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Surname ?? string.Empty))
            .ForMember(dest => dest.Image, opt => opt.MapFrom(src =>
                src.Avatar != null ? src.Avatar.StorageKey : string.Empty
            ))
            .ForMember(dest => dest.IsBlocked, opt => opt.MapFrom(src => src.IsBlocked));

        // Map from Domain User to UserDto
        // This mapping uses context items for identity-specific fields (Username, Email, PhoneNumber, Roles, AvatarUrl)
        // Since UserDto is a record, we use constructor parameters with context items
        CreateMap<User, UserDto>()
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.IdentityUserId))
            .ForCtorParam("Username", opt => opt.MapFrom((src, ctx) =>
                ctx.Items.TryGetValue("Username", out var username) ? username as string ?? string.Empty : string.Empty))
            .ForCtorParam("Name", opt => opt.MapFrom(src => src.Name ?? string.Empty))
            .ForCtorParam("Surname", opt => opt.MapFrom(src => src.Surname ?? string.Empty))
            .ForCtorParam("Email", opt => opt.MapFrom((src, ctx) =>
                ctx.Items.TryGetValue("Email", out var email) ? email as string ?? string.Empty : string.Empty))
            .ForCtorParam("PhoneNumber", opt => opt.MapFrom((src, ctx) =>
                ctx.Items.TryGetValue("PhoneNumber", out var phone) ? phone as string ?? string.Empty : string.Empty))
            .ForCtorParam("Roles", opt => opt.MapFrom((src, ctx) =>
                ctx.Items.TryGetValue("Roles", out var roles) ? roles as List<string> ?? new List<string>() : new List<string>()))
            .ForCtorParam("AvatarUrl", opt => opt.MapFrom((src, ctx) =>
                ctx.Items.TryGetValue("AvatarUrl", out var avatarUrl) ? avatarUrl as string : null));

        // Mapping from UserDto to Domain User is not direct; handlers should coordinate domain + identity updates.
    }
}