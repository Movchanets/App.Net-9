using System;
using Application.Interfaces;
using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Infrastructure.Data.Constants;
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
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;

	public UserService(
		UserManager<ApplicationUser> userManager,
		RoleManager<RoleEntity> roleManager,
		IUserRepository userRepository,
		IFileStorage fileStorage,
		IImageService imageService,
		IMediaImageRepository mediaImageRepository,
		IUnitOfWork unitOfWork,
		IMapper mapper)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_userRepository = userRepository;
		_fileStorage = fileStorage;
		_imageService = imageService;
		_mediaImageRepository = mediaImageRepository;
		_unitOfWork = unitOfWork;
		_mapper = mapper;
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
		RegistrationDto registrationModel)
	{
		using var transaction = await _unitOfWork.BeginTransactionAsync();
		try
		{
			var user = new ApplicationUser
			{
				Email = registrationModel.Email
			};

			string baseUsername = ($"{registrationModel.Name}.{registrationModel.Surname}").ToLower();
			baseUsername = System.Text.RegularExpressions.Regex.Replace(baseUsername, ValidationRegexPattern.UsernameSanitizePattern, "");
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
			{
				await transaction.RollbackAsync();
				return (false, result.Errors.Select(e => e.Description).ToList(), null);
			}
			var domainUser = new User(user.Id, registrationModel.Name, registrationModel.Surname, registrationModel.Email);
			_userRepository.Add(domainUser);
			user.DomainUserId = domainUser.Id;
			await _userManager.UpdateAsync(user);
			await _unitOfWork.SaveChangesAsync();
			await transaction.CommitAsync();
			return (true, new List<string>(), user.Id);
		}
		catch
		{
			await transaction.RollbackAsync();
			throw;
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
		using var transaction = await _unitOfWork.BeginTransactionAsync();
		try
		{
			var user = await _userManager.FindByIdAsync(identityUserId.ToString());
			if (user == null) return null;
			if (!string.IsNullOrWhiteSpace(username)) user.UserName = username;
			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				await transaction.RollbackAsync();
				return null;
			}
			var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
			if (domain != null && (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName)))
			{
				domain.UpdateProfile(firstName ?? domain.Name, lastName ?? domain.Surname);
				_userRepository.Update(domain);
			}
			await _unitOfWork.SaveChangesAsync();
			await transaction.CommitAsync();
			return await TryBuildUserDtoAsync(user);
		}
		catch
		{
			await transaction.RollbackAsync();
			throw;
		}
	}

	public async Task<UserDto?> UpdatePhoneAsync(Guid identityUserId, string phone)
	{
		using var transaction = await _unitOfWork.BeginTransactionAsync();
		try
		{
			var user = await _userManager.FindByIdAsync(identityUserId.ToString());
			if (user == null) return null;

			user.PhoneNumber = phone;
			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				await transaction.RollbackAsync();
				return null;
			}

			// Update domain user phone number
			var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
			if (domain != null)
			{
				domain.UpdatePhoneNumber(phone);
				_userRepository.Update(domain);
			}

			await _unitOfWork.SaveChangesAsync();
			await transaction.CommitAsync();

			return await TryBuildUserDtoAsync(user);
		}
		catch
		{
			await transaction.RollbackAsync();
			throw;
		}
	}

	public async Task<UserDto?> UpdateEmailAsync(Guid identityUserId, string email)
	{
		using var transaction = await _unitOfWork.BeginTransactionAsync();
		try
		{
			var user = await _userManager.FindByIdAsync(identityUserId.ToString());
			if (user == null) return null;

			user.Email = email;
			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				await transaction.RollbackAsync();
				return null;
			}

			// Update domain user email
			var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
			if (domain != null)
			{
				domain.UpdateEmail(email);
				_userRepository.Update(domain);
			}

			await _unitOfWork.SaveChangesAsync();
			await transaction.CommitAsync();

			return await TryBuildUserDtoAsync(user);
		}
		catch
		{
			await transaction.RollbackAsync();
			throw;
		}
	}

	public async Task<UserDto?> UpdateProfileInfoAsync(Guid identityUserId, string? username, string? firstName, string? lastName)
	{
		using var transaction = await _unitOfWork.BeginTransactionAsync();
		try
		{
			var user = await _userManager.FindByIdAsync(identityUserId.ToString());
			if (user == null) return null;

			if (!string.IsNullOrWhiteSpace(username)) user.UserName = username;
			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				await transaction.RollbackAsync();
				return null;
			}

			var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
			if (domain != null && (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName)))
			{
				domain.UpdateProfile(firstName ?? domain.Name, lastName ?? domain.Surname);
				_userRepository.Update(domain);
			}

			await _unitOfWork.SaveChangesAsync();
			await transaction.CommitAsync();

			return await TryBuildUserDtoAsync(user);
		}
		catch
		{
			await transaction.RollbackAsync();
			throw;
		}
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
		using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
		try
		{
			var user = await _userManager.FindByIdAsync(identityUserId.ToString());
			if (user == null) return null;

			var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
			if (domain == null) return null;

			var processed = await _imageService.ProcessAsync(fileStream, ImageResizeMode.Thumbnail, 256, 256, cancellationToken);
			var storageKey = await _fileStorage.UploadAsync(processed.ImageStream, fileName, processed.ContentType, cancellationToken);
			var publicUrl = _fileStorage.GetPublicUrl(storageKey);

			if (domain.Avatar != null && !string.IsNullOrWhiteSpace(domain.Avatar.StorageKey))
			{
				try { await _fileStorage.DeleteAsync(domain.Avatar.StorageKey, cancellationToken); } catch { }
			}

			var mediaImage = new MediaImage(
				storageKey: storageKey,
				mimeType: processed.ContentType,
				width: processed.Width,
				height: processed.Height,
				altText: $"Avatar for {domain.Name} {domain.Surname}"
			);

			_mediaImageRepository.Add(mediaImage);
			domain.SetAvatar(mediaImage);
			_userRepository.Update(domain);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);
			return await TryBuildUserDtoAsync(user);
		}
		catch
		{
			await transaction.RollbackAsync(cancellationToken);
			throw;
		}
	}

	public async Task<UserDto?> DeleteProfilePictureAsync(Guid identityUserId, CancellationToken cancellationToken = default)
	{
		using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
		try
		{
			var user = await _userManager.FindByIdAsync(identityUserId.ToString());
			if (user == null) return null;

			var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
			if (domain == null) return null;

			// Delete file from storage if exists
			if (domain.Avatar != null && !string.IsNullOrWhiteSpace(domain.Avatar.StorageKey))
			{
				try
				{
					await _fileStorage.DeleteAsync(domain.Avatar.StorageKey, cancellationToken);
				}
				catch
				{
					// Log but don't fail if file deletion fails
				}

				// Remove avatar reference
				domain.RemoveAvatar();
				_userRepository.Update(domain);
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);
			return await TryBuildUserDtoAsync(user);
		}
		catch
		{
			await transaction.RollbackAsync(cancellationToken);
			throw;
		}
	}

	private async Task<UserDto?> TryBuildUserDtoAsync(ApplicationUser user)
	{
		var domain = await _userRepository.GetByIdentityUserIdAsync(user.Id);
		if (domain == null) return null;

		var roles = (await _userManager.GetRolesAsync(user)).ToList();

		// Build avatar URL if available
		string? avatarUrl = null;
		if (domain.Avatar != null && !string.IsNullOrWhiteSpace(domain.Avatar.StorageKey))
		{
			avatarUrl = _fileStorage.GetPublicUrl(domain.Avatar.StorageKey);
		}

		// Use AutoMapper with context items for identity fields
		var userDto = _mapper.Map<UserDto>(domain, opts =>
		{
			opts.Items["Username"] = user.UserName ?? string.Empty;
			opts.Items["Email"] = user.Email ?? string.Empty;
			opts.Items["PhoneNumber"] = user.PhoneNumber ?? string.Empty;
			opts.Items["Roles"] = roles;
			opts.Items["AvatarUrl"] = avatarUrl;
		});

		return userDto;
	}

}
