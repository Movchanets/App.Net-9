using Application.DTOs;
using Application.Mapping;
using AutoMapper;
using FluentAssertions;
using Infrastructure.Data.Models;
using Infrastructure.Entities;
using Microsoft.Extensions.Logging.Abstractions;

public class AutoMapperTest
{
    private static IMapper _mapper;

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
        IMapper mapper = mappingConfig.CreateMapper();
        _mapper = mapper;

    }

    [Fact]
    public void UserDto_To_UserEntity_Mapping_Works()
    {
        // Arrange
        var dto = new UserDto("jdoe", "John", "Doe", "jdoe@example.com", string.Empty, new List<string> { "User", "Admin" });

        // Act
        var entity = _mapper.Map<UserEntity>(dto);

        // Assert
        entity.Should().NotBeNull();
        entity.UserName.Should().Be("jdoe");
        entity.Name.Should().Be("John");
        entity.Surname.Should().Be("Doe");
        entity.Email.Should().Be("jdoe@example.com");
        // UserRoles is ignored by mapping and should remain the default empty collection
        entity.UserRoles.Should().NotBeNull();
        entity.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void UserEntity_To_UserDto_Mapping_Maps_Roles_And_Basics()
    {
        // Arrange
        var user = new UserEntity
        {
            UserName = "alice",
            Name = "Alice",
            Surname = "Smith",
            Email = "alice@example.com",
            UserRoles = new List<UserRoleEntity>
            {
                new UserRoleEntity { Role = new RoleEntity { Name = "User" } },
                new UserRoleEntity { Role = new RoleEntity { Name = "Admin" } }
            }
        };

        // Act
        var dto = _mapper.Map<UserDto>(user);

        // Assert
        dto.Should().NotBeNull();
        dto.Username.Should().Be("alice");
        dto.Name.Should().Be("Alice");
        dto.Surname.Should().Be("Smith");
        dto.Email.Should().Be("alice@example.com");
        dto.PhoneNumber.Should().Be(string.Empty);
        dto.Roles.Should().BeEquivalentTo(new[] { "User", "Admin" });
    }

    [Fact]
    public void UserEntity_To_UserVM_Maps_Image_And_UserRoles_And_Conventional_Props()
    {
        // Arrange
        var user = new UserEntity
        {
            Id = 42,
            UserName = "bob",
            Email = "bob@example.com",
            ImageUrl = "https://example.com/avatar.png",
            IsBlocked = true,
            PhoneNumber = "123456",
            UserRoles = new List<UserRoleEntity>
            {
                new UserRoleEntity { Role = new RoleEntity { Name = "User" } }
            }
        };

        // Act
        var vm = _mapper.Map<UserVM>(user);

        // Assert
        vm.Should().NotBeNull();
        vm.Id.Should().Be(42);
        vm.UserName.Should().Be("bob");
        vm.Email.Should().Be("bob@example.com");
        vm.Image.Should().Be("https://example.com/avatar.png");
        vm.IsBlocked.Should().BeTrue();
        vm.PhoneNumber.Should().Be("123456");
        vm.UserRoles.Should().ContainSingle().Which.Should().Be("User");
    }
}

