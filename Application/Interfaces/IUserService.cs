using System;
using Domain.Entities;
using Application.DTOs;

namespace Application.Interfaces;

public interface IUserService
{

	Task<bool> ValidatePasswordAsync(string email, string password);
	Task<User?> FindUserByEmailAsync(string email);
	Task<Guid?> ValidateRefreshTokenAsync(string refreshToken);
	Task<bool> CreateRoleAsync(string roleName);
	Task<bool> DeleteUserByIdAsync(Guid id);
	Task<bool> EmailExistsAsync(string email);
	Task<UserDto?> GetIdentityInfoByIdAsync(Guid identityUserId);
	Task<(bool Succeeded, List<string> Errors, Guid? IdentityUserId)> RegisterAsync(string email, string password, string firstName, string lastName);
	Task<bool> EnsureRoleExistsAsync(string roleName);
	Task<bool> AddUserToRoleAsync(Guid identityUserId, string roleName);
	Task<string?> GeneratePasswordResetTokenAsync(string email);
	Task<bool> UpdateSecurityStampByEmailAsync(string email);
	Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
	Task<bool> ChangePasswordAsync(Guid identityUserId, string currentPassword, string newPassword);
	Task<UserDto?> UpdateIdentityProfileAsync(Guid identityUserId, string? username, string? email, string? phone, string? firstName, string? lastName);
	Task<UserDto?> GetIdentityInfoByEmailAsync(string email);
	Task<List<UserDto>> GetAllUsersAsync();

}
