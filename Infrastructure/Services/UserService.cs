using System;
using Application.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Infrastructure.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class UserService : IUserService
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly RoleManager<RoleEntity> _roleManager;
	private readonly IUserRepository _userRepository;

	public UserService(UserManager<ApplicationUser> userManager, RoleManager<RoleEntity> roleManager, IUserRepository userRepository)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_userRepository = userRepository;
	}

	public async Task<bool> ValidatePasswordAsync(string email, string password)
	{
		var identityUser = await _userManager.FindByEmailAsync(email);
		if (identityUser == null)
			return false;
		return await _userManager.CheckPasswordAsync(identityUser, password);
	}

	public async Task<User?> FindUserByEmailAsync(string email)
	{
		var identityUser = await _userManager.FindByEmailAsync(email);
		if (identityUser == null)
			return null;
		return await _userRepository.GetByIdentityUserIdAsync(identityUser.Id);
	}

	public async Task<Guid?> ValidateRefreshTokenAsync(string refreshToken)
	{
		var identityUser = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
		if (identityUser == null)
			return null;
		if (!identityUser.RefreshTokenExpiryTime.HasValue || identityUser.RefreshTokenExpiryTime.Value < DateTime.UtcNow)
			return null;
		return identityUser.Id;
	}

	public async Task<bool> CreateRoleAsync(string roleName)
	{
		if (await _roleManager.RoleExistsAsync(roleName))
			return false;
		var role = new RoleEntity { Name = roleName };
		var result = await _roleManager.CreateAsync(role);
		return result.Succeeded;
	}

	public async Task<bool> DeleteUserByIdAsync(Guid id)
	{
		var identityUser = await _userManager.FindByIdAsync(id.ToString());
		if (identityUser == null) return false;
		var result = await _userManager.DeleteAsync(identityUser);
		return result.Succeeded;
	}

	public async Task<bool> EmailExistsAsync(string email)
	{
		var identityUser = await _userManager.FindByEmailAsync(email);
		return identityUser != null;
	}

	public async Task<UserDto?> GetIdentityInfoByIdAsync(Guid identityUserId)
	{
		var u = await _userManager.FindByIdAsync(identityUserId.ToString());
		if (u == null) return null;
		var roles = (await _userManager.GetRolesAsync(u)).ToList();
		var domain = await _userRepository.GetByIdentityUserIdAsync(u.Id);
		var name = domain?.Name ?? string.Empty;
		var surname = domain?.Surname ?? string.Empty;
		return new UserDto(
			u.Id,
			u.UserName ?? string.Empty,
			name,
			surname,
			u.Email ?? string.Empty,
			u.PhoneNumber ?? string.Empty,
			roles
		);
	}

	public async Task<(bool Succeeded, List<string> Errors, Guid? IdentityUserId)> RegisterAsync(string email, string password, string firstName, string lastName)
	{
		var user = new ApplicationUser
		{
			Email = email
		};

		// ... (весь ваш код генерації UserName ... залишається без змін) ...
		string baseUsername = ($"{firstName}.{lastName}").ToLower();
		baseUsername = System.Text.RegularExpressions.Regex.Replace(baseUsername, "[^a-z0-9._-]", "");
		if (string.IsNullOrWhiteSpace(baseUsername)) baseUsername = "user";
		string username = baseUsername;
		int counter = 1;
		while (await _userManager.FindByNameAsync(username) != null)
		{
			counter++;
			username = baseUsername + counter;
		}
		user.UserName = username;

		var result = await _userManager.CreateAsync(user, password);
		if (!result.Succeeded)
			return (false, result.Errors.Select(e => e.Description).ToList(), null);
		var domainUser = new User(user.Id, firstName, lastName);
		await _userRepository.AddAsync(domainUser);
		user.DomainUserId = domainUser.Id;
		await _userManager.UpdateAsync(user);


		return (true, new List<string>(), user.Id);
	}

	public async Task<bool> EnsureRoleExistsAsync(string roleName)
	{
		if (await _roleManager.RoleExistsAsync(roleName))
			return true;
		var role = new RoleEntity { Name = roleName };
		var result = await _roleManager.CreateAsync(role);
		return result.Succeeded;
	}

	public async Task<bool> AddUserToRoleAsync(Guid identityUserId, string roleName)
	{
		var user = await _userManager.FindByIdAsync(identityUserId.ToString());
		if (user == null) return false;
		var result = await _userManager.AddToRoleAsync(user, roleName);
		return result.Succeeded;
	}

	public async Task<string?> GeneratePasswordResetTokenAsync(string email)
	{
		var user = await _userManager.FindByEmailAsync(email);
		if (user == null) return null;
		return await _userManager.GeneratePasswordResetTokenAsync(user);
	}

	public async Task<bool> UpdateSecurityStampByEmailAsync(string email)
	{
		var user = await _userManager.FindByEmailAsync(email);
		if (user == null) return false;
		await _userManager.UpdateSecurityStampAsync(user);
		return true;
	}

	public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
	{
		var user = await _userManager.FindByEmailAsync(email);
		if (user == null) return false;
		var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
		return result.Succeeded;
	}

	public async Task<bool> ChangePasswordAsync(Guid identityUserId, string currentPassword, string newPassword)
	{
		var user = await _userManager.FindByIdAsync(identityUserId.ToString());
		if (user == null) return false;
		var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
		return result.Succeeded;
	}

	public async Task<UserDto?> UpdateIdentityProfileAsync(Guid identityUserId, string? username, string? email, string? phone, string? firstName, string? lastName)
	{
		var user = await _userManager.FindByIdAsync(identityUserId.ToString());
		if (user == null) return null;
		if (!string.IsNullOrWhiteSpace(username)) user.UserName = username;
		if (!string.IsNullOrWhiteSpace(email)) user.Email = email;
		if (!string.IsNullOrWhiteSpace(phone)) user.PhoneNumber = phone;
		var result = await _userManager.UpdateAsync(user);
		if (!result.Succeeded) return null;
		// Update domain user names when provided
		var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
		if (domain != null && (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName)))
		{
			domain.UpdateProfile(firstName ?? domain.Name, lastName ?? domain.Surname, domain.ImageUrl);
			await _userRepository.UpdateAsync(domain);
		}
		var roles = (await _userManager.GetRolesAsync(user)).ToList();
		var name = domain?.Name ?? string.Empty;
		var surname = domain?.Surname ?? string.Empty;
		return new UserDto(
			user.Id,
			user.UserName ?? string.Empty,
			firstName ?? name,
			lastName ?? surname,
			user.Email ?? string.Empty,
			user.PhoneNumber ?? string.Empty,
			roles
		);
	}
	public async Task<UserDto?> GetIdentityInfoByEmailAsync(string email)
	{
		var user = await _userManager.FindByEmailAsync(email);
		if (user == null) return null;
		var roles = (await _userManager.GetRolesAsync(user)).ToList();
		var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
		var name = domain?.Name ?? string.Empty;
		var surname = domain?.Surname ?? string.Empty;
		return new UserDto(
			user.Id,
			user.UserName ?? string.Empty,
			name,
			surname,
			user.Email ?? string.Empty,
			user.PhoneNumber ?? string.Empty,
			roles
		);
	}

	public async Task<List<UserDto>> GetAllUsersAsync()
	{
		var list = new List<UserDto>();
		foreach (var user in _userManager.Users)
		{
			var roles = (await _userManager.GetRolesAsync(user)).ToList();
			var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
			var name = domain?.Name ?? string.Empty;
			var surname = domain?.Surname ?? string.Empty;
			list.Add(new UserDto(
				user.Id,
				user.UserName ?? string.Empty,
				name,
				surname,
				user.Email ?? string.Empty,
				user.PhoneNumber ?? string.Empty,
				roles
			));
		}
		return list;
	}
}
