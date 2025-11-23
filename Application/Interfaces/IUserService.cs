using System;
using Domain.Entities;
using Application.DTOs;
using Application.ViewModels;

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
	Task<(bool Succeeded, List<string> Errors, Guid? IdentityUserId)> RegisterAsync(RegistrationVM registrationModel);
	Task<bool> EnsureRoleExistsAsync(string roleName);
	Task<bool> AddUserToRoleAsync(Guid identityUserId, string roleName);
	Task<string?> GeneratePasswordResetTokenAsync(string email);
	Task<bool> UpdateSecurityStampByEmailAsync(string email);
	Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
	Task<bool> ChangePasswordAsync(Guid identityUserId, string currentPassword, string newPassword);
	Task<UserDto?> UpdateIdentityProfileAsync(Guid identityUserId, string? username, string? firstName, string? lastName);
	// Granular profile updates
	Task<UserDto?> UpdatePhoneAsync(Guid identityUserId, string phone);
	Task<UserDto?> UpdateEmailAsync(Guid identityUserId, string email);
	Task<UserDto?> UpdateProfileInfoAsync(Guid identityUserId, string? username, string? firstName, string? lastName);
	Task<UserDto?> GetIdentityInfoByEmailAsync(string email);
	Task<List<UserDto>> GetAllUsersAsync();
	Task<UserDto?> UpdateProfilePictureAsync(Guid identityUserId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
	Task<UserDto?> DeleteProfilePictureAsync(Guid identityUserId, CancellationToken cancellationToken = default);

}
