using System;
using System.Security.Claims;
using Application.Queries.User;
using Application.Queries.User.GetUserByEmail;
using Application.Queries.User.GetUserById;
using Application.Queries.User.GetUsers;
using Application.Interfaces;
using Infrastructure.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using API.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Application.Commands.User.DeleteUser;
using Application.ViewModels;
using Application.Queries.User.GetProfile;
using Application.Commands.User.Profile.UpdateProfile;
using Application.Commands.User.Profile.ChangePassword;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace API.Controllers;

[Authorize]
[ServiceFilter(typeof(TurnstileValidationFilter))]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, UserManager<ApplicationUser> userManager, ITokenService tokenService, IMemoryCache memoryCache, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _userManager = userManager;
        _tokenService = tokenService; // keep field for potential use
        _logger = logger;
    }

    /// <summary>
    /// Отримання користувача за ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:users.read")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var result = await _mediator.Send(new GetUserQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Отримання всіх користувачів
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Permission:users.read")]
    public async Task<IActionResult> GetUsers()
    {
        var result = await _mediator.Send(new GetUsersQuery());
        if (result.Payload == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Отримання користувача за email
    /// </summary>
    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var result = await _mediator.Send(new GetUserByEmailQuery(email));
        return Ok(result);
    }

    /// <summary>
    /// Отримати профіль поточного користувача
    /// </summary>
    [HttpGet("me")]
    [Authorize(Policy = "Permission:profile.read.self")]
    public async Task<IActionResult> GetMyProfile()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim)) return Unauthorized();
        if (!Guid.TryParse(idClaim, out var userId)) return Unauthorized();

        var result = await _mediator.Send(new GetProfileQuery(userId));
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Оновлення профілю поточного користувача (name, surname, username, phone)
    /// </summary>
    [Obsolete("Use /me/info, /me/phone, or /me/email instead")]
    [HttpPut("me")]
    [Authorize(Policy = "Permission:profile.update.self")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileVM data)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim)) return Unauthorized();
        if (!Guid.TryParse(idClaim, out var userId)) return Unauthorized();

        var result = await _mediator.Send(new UpdateProfileCommand(userId, data));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Оновлення телефону поточного користувача
    /// </summary>
    [HttpPut("me/phone")]
    [Authorize(Policy = "Permission:profile.update.self")]
    public async Task<IActionResult> UpdateMyPhone([FromBody] UpdatePhoneVM data)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim)) return Unauthorized();
        if (!Guid.TryParse(idClaim, out var userId)) return Unauthorized();

        var result = await _mediator.Send(new Application.Commands.User.Profile.UpdatePhone.UpdatePhoneCommand(userId, data));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Оновлення email поточного користувача
    /// </summary>
    [HttpPut("me/email")]
    [Authorize(Policy = "Permission:profile.update.self")]
    public async Task<IActionResult> UpdateMyEmail([FromBody] UpdateEmailVM data)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim)) return Unauthorized();
        if (!Guid.TryParse(idClaim, out var userId)) return Unauthorized();

        var result = await _mediator.Send(new Application.Commands.User.Profile.UpdateEmail.UpdateEmailCommand(userId, data));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Оновлення імені/прізвища/нікнейму поточного користувача
    /// </summary>
    [HttpPut("me/info")]
    [Authorize(Policy = "Permission:profile.update.self")]
    public async Task<IActionResult> UpdateMyInfo([FromBody] UpdateProfileInfoVM data)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim)) return Unauthorized();
        if (!Guid.TryParse(idClaim, out var userId)) return Unauthorized();

        var result = await _mediator.Send(new Application.Commands.User.Profile.UpdateProfileInfo.UpdateProfileInfoCommand(userId, data));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Зміна паролю поточного користувача
    /// </summary>
    [HttpPut("me/password")]
    [Authorize(Policy = "Permission:profile.update.self")]
    public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordVM data)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim)) return Unauthorized();
        if (!Guid.TryParse(idClaim, out var userId)) return Unauthorized();

        var result = await _mediator.Send(new ChangePasswordCommand(userId, data));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Завантаження фото профілю поточного користувача
    /// </summary>
    [HttpPost("me/picture")]
    [Authorize(Policy = "Permission:profile.update.self")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim)) return Unauthorized();
        if (!Guid.TryParse(idClaim, out var userId)) return Unauthorized();

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { IsSuccess = false, Message = "File is required" });
        }

        _logger.LogInformation("User {UserId} is uploading profile picture: {FileName}, {Size} bytes", userId, file.FileName, file.Length);

        using var stream = file.OpenReadStream();
        var result = await _mediator.Send(new Application.Commands.User.Profile.UploadProfilePicture.UploadProfilePictureCommand(
            userId,
            stream,
            file.FileName,
            file.ContentType
        ));

        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Видалення фото профілю поточного користувача
    /// </summary>
    [HttpDelete("me/picture")]
    [Authorize(Policy = "Permission:profile.update.self")]
    public async Task<IActionResult> DeleteProfilePicture()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(idClaim)) return Unauthorized();
        if (!Guid.TryParse(idClaim, out var userId)) return Unauthorized();

        _logger.LogInformation("User {UserId} is deleting profile picture", userId);

        var result = await _mediator.Send(new Application.Commands.User.Profile.DeleteProfilePicture.DeleteProfilePictureCommand(userId));

        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Видалення користувача
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:users.delete")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _mediator.Send(new DeleteUserCommand(id));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
