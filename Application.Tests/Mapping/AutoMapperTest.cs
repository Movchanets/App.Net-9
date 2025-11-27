using Application.DTOs;
using Application.Mapping;
using AutoMapper;
using FluentAssertions;
using Application.Models;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

public class AutoMapperTest
{
    private readonly IMapper _mapper;

    public AutoMapperTest()
    {
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new AutoMapperProfile());
            // Enable constructor mapping for AutoMapper v15 so records with positional constructors are supported
            try
            {
                var enableMethod = mc.GetType().GetMethod("EnableConstructorMapping");
                if (enableMethod != null)
                    enableMethod.Invoke(mc, null);
            }
            catch
            {
                // ignore if method not available in this AutoMapper version
            }
        }, NullLoggerFactory.Instance);
        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void DomainUser_To_UserDto_Mapping_Works()
    {
        // Arrange - Domain.User містить лише Name, Surname, ImageUrl
        var domainUser = new User(System.Guid.NewGuid(), "John", "Doe");

        // Act - передаємо context items для Identity полів
        var dto = _mapper.Map<UserDto>(domainUser, opts =>
        {
            opts.Items["Username"] = "john.doe";
            opts.Items["Email"] = "john@example.com";
            opts.Items["PhoneNumber"] = "+1234567890";
            opts.Items["Roles"] = new List<string> { "Customer" };
            opts.Items["AvatarUrl"] = "/uploads/avatar.jpg";
        });

        // Assert - базовий маппінг працює з context items
        dto.Should().NotBeNull();
        dto.Name.Should().Be("John");
        dto.Surname.Should().Be("Doe");
        dto.Username.Should().Be("john.doe");
        dto.Email.Should().Be("john@example.com");
        dto.PhoneNumber.Should().Be("+1234567890");
        dto.Roles.Should().ContainSingle().Which.Should().Be("Customer");
        dto.AvatarUrl.Should().Be("/uploads/avatar.jpg");
    }

    [Fact]
    public void DomainUser_To_UserVM_Mapping_Works()
    {
        // Arrange
        var id = System.Guid.NewGuid();
        var domainUser = new User(id, "Alice", "Smith");

        // Act
        var vm = _mapper.Map<UserVM>(domainUser);

        // Assert
        vm.Should().NotBeNull();
        vm.Id.Should().Be(id);
        vm.FirstName.Should().Be("Alice");
        vm.LastName.Should().Be("Smith");
        vm.Image.Should().BeEmpty(); // Mapped from ImageUrl
        vm.IsBlocked.Should().BeFalse(); // Default value from Domain.User
    }
}


