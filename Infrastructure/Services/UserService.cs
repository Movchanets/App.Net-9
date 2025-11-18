using System;
using Application.Interfaces;
using Application.DTOs;
using Application.ViewModels;
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
	private readonly IFileStorage _fileStorage;
	private readonly IImageService _imageService;
	private readonly IMediaImageRepository _mediaImageRepository;

	public UserService(
		UserManager<ApplicationUser> userManager,
		RoleManager<RoleEntity> roleManager,
		IUserRepository userRepository,
		IFileStorage fileStorage,
		IImageService imageService,
		IMediaImageRepository mediaImageRepository)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_userRepository = userRepository;
		_fileStorage = fileStorage;
		_imageService = imageService;
		_mediaImageRepository = mediaImageRepository;
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

		return await TryBuildUserDtoAsync(u);
	}

	public async Task<(bool Succeeded, List<string> Errors, Guid? IdentityUserId)> RegisterAsync(
		RegistrationVM registrationModel)
	{

		{
			var user = new ApplicationUser
			{
				Email = registrationModel.Email
			};

			// ... (весь ваш код генерації UserName ... залишається без змін) ...
			string baseUsername = ($"{registrationModel.Name}.{registrationModel.Surname}").ToLower();
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

			var result = await _userManager.CreateAsync(user, registrationModel.Password);
			if (!result.Succeeded)
				return (false, result.Errors.Select(e => e.Description).ToList(), null);
			var domainUser = new User(user.Id, registrationModel.Name, registrationModel.Surname);
			await _userRepository.AddAsync(domainUser);
			user.DomainUserId = domainUser.Id;
			await _userManager.UpdateAsync(user);


			return (true, new List<string>(), user.Id);
		}
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

	public async Task<UserDto?> UpdateIdentityProfileAsync(Guid identityUserId, string? username, string? firstName,
		string? lastName)
	{
		var user = await _userManager.FindByIdAsync(identityUserId.ToString());
		if (user == null) return null;
		if (!string.IsNullOrWhiteSpace(username)) user.UserName = username;
		var result = await _userManager.UpdateAsync(user);
		if (!result.Succeeded) return null;
		// Update domain user names when provided
		var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
		if (domain != null && (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName)))
		{
			domain.UpdateProfile(firstName ?? domain.Name, lastName ?? domain.Surname);
			await _userRepository.UpdateAsync(domain);
		}

		return await TryBuildUserDtoAsync(user);
	}

	public async Task<UserDto?> UpdatePhoneAsync(Guid identityUserId, string phone)
	{
		var user = await _userManager.FindByIdAsync(identityUserId.ToString());
		if (user == null) return null;

		user.PhoneNumber = phone;
		var result = await _userManager.UpdateAsync(user);
		if (!result.Succeeded) return null;

		return await TryBuildUserDtoAsync(user);
	}

	public async Task<UserDto?> UpdateEmailAsync(Guid identityUserId, string email)
	{
		var user = await _userManager.FindByIdAsync(identityUserId.ToString());
		if (user == null) return null;

		user.Email = email;
		var result = await _userManager.UpdateAsync(user);
		if (!result.Succeeded) return null;


		return await TryBuildUserDtoAsync(user);
	}

	public async Task<UserDto?> UpdateProfileInfoAsync(Guid identityUserId, string? username, string? firstName, string? lastName)
	{
		var user = await _userManager.FindByIdAsync(identityUserId.ToString());
		if (user == null) return null;

		if (!string.IsNullOrWhiteSpace(username)) user.UserName = username;
		var result = await _userManager.UpdateAsync(user);
		if (!result.Succeeded) return null;

		// Update domain user names when provided
		var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
		if (domain != null && (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName)))
		{
			domain.UpdateProfile(firstName ?? domain.Name, lastName ?? domain.Surname);
			await _userRepository.UpdateAsync(domain);
		}

		var roles = (await _userManager.GetRolesAsync(user)).ToList();
		var name = domain?.Name ?? string.Empty;
		var surname = domain?.Surname ?? string.Empty;

		// Build avatar URL if available
		string? avatarUrl = null;
		if (domain?.Avatar != null && !string.IsNullOrWhiteSpace(domain.Avatar.StorageKey))
		{
			avatarUrl = _fileStorage.GetPublicUrl(domain.Avatar.StorageKey);
		}

		return new UserDto(
			user.Id,
			user.UserName ?? string.Empty,
			firstName ?? name,
			lastName ?? surname,
			user.Email ?? string.Empty,
			user.PhoneNumber ?? string.Empty,
			roles,
			avatarUrl
		);
	}
	public async Task<UserDto?> GetIdentityInfoByEmailAsync(string email)
	{
		var user = await _userManager.FindByEmailAsync(email);
		if (user == null) return null;
		return await TryBuildUserDtoAsync(user);
	}

	public async Task<List<UserDto>> GetAllUsersAsync()
	{
		var list = new List<UserDto>();
		foreach (var user in _userManager.Users)
		{
			var dto = await TryBuildUserDtoAsync(user);
			if (dto != null)
			{
				list.Add(dto);
			}
		}
		return list;
	}

	public async Task<UserDto?> UpdateProfilePictureAsync(Guid identityUserId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(identityUserId.ToString());
		if (user == null) return null;

		var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
		if (domain == null) return null;

		// Process image: resize to thumbnail, convert to WebP, remove metadata
		var processed = await _imageService.ProcessAsync(fileStream, ImageResizeMode.Thumbnail, 256, 256, cancellationToken);

		// Upload processed image to storage
		var storageKey = await _fileStorage.UploadAsync(processed.ImageStream, fileName, processed.ContentType, cancellationToken);

		// Get public URL
		var publicUrl = _fileStorage.GetPublicUrl(storageKey);

		// Delete old avatar if exists
		if (domain.Avatar != null && !string.IsNullOrWhiteSpace(domain.Avatar.StorageKey))
		{
			try
			{
				await _fileStorage.DeleteAsync(domain.Avatar.StorageKey, cancellationToken);
			}
			catch
			{
				// Log but don't fail if old avatar deletion fails
			}
		}

		// Create MediaImage entity
		var mediaImage = new MediaImage(
			storageKey: storageKey,
			mimeType: processed.ContentType,
			width: processed.Width,
			height: processed.Height,
			altText: $"Avatar for {domain.Name} {domain.Surname}"
		);

		// Save MediaImage to database first (to satisfy FK constraint)
		await _mediaImageRepository.AddAsync(mediaImage);

		// Update domain entity
		domain.SetAvatar(mediaImage);
		await _userRepository.UpdateAsync(domain);

		return await TryBuildUserDtoAsync(user);
	}

	private async Task<UserDto?> TryBuildUserDtoAsync(ApplicationUser user)
	{
		var roles = (await _userManager.GetRolesAsync(user)).ToList();
		var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
		if (domain == null) return null;

		// Build avatar URL if available
		string? avatarUrl = null;
		if (domain.Avatar != null && !string.IsNullOrWhiteSpace(domain.Avatar.StorageKey))
		{
			avatarUrl = _fileStorage.GetPublicUrl(domain.Avatar.StorageKey);
		}

		return new UserDto(
			user.Id,
			user.UserName ?? string.Empty,
			domain.Name ?? string.Empty,
			domain.Surname ?? string.Empty,
			user.Email ?? string.Empty,
			user.PhoneNumber ?? string.Empty,
			roles,
			avatarUrl
		);
	}

}
