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

        // Act
        var dto = _mapper.Map<UserDto>(domainUser);

        // Assert - базовий маппінг працює, але Username/Email/Roles заповнюються handler'ом
        dto.Should().NotBeNull();
        dto.Name.Should().Be("John");
        dto.Surname.Should().Be("Doe");
        dto.Username.Should().Be(string.Empty); // Заповнюється handler'ом з IIdentityService
        dto.Email.Should().Be(string.Empty); // Заповнюється handler'ом з IIdentityService
        dto.Roles.Should().BeEmpty(); // Заповнюється handler'ом з IIdentityService
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


