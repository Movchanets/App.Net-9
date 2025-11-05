using System.Security.Claims;
using Application.Commands.User.CreateUser;
using System.Net;
using API.Services;
using Application.DTOs;
using Application.Interfaces;
using Application.Queries.User;
using Application.Queries.User.GetUserByEmail;
using Application.Queries.User.GetUserById;
using Application.Queries.User.GetUsers;
using Application.ViewModels;
using AutoMapper;
using Infrastructure.Data.Models;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<UserEntity> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IEmailQueue _emailQueue;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, UserManager<UserEntity> userManager, ITokenService tokenService, IEmailService emailService, IEmailQueue emailQueue, IMemoryCache memoryCache, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        this._userManager = userManager;
        this._tokenService = tokenService;
        _emailService = emailService;
        _emailQueue = emailQueue;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <summary>
    /// Тестовий endpoint для отримання токенів
    /// </summary>
    [AllowAnonymous]
    [HttpGet("test-tokens")]
    public async Task<IActionResult> GetTokensTest()
    {
        var user = await _userManager.FindByIdAsync("1");
        if (user == null) return NotFound("User not found");
      var tokens = await _tokenService.GenerateTokensAsync(user);
        return Ok(tokens);

    }

    /// <summary>
    /// Аутентифікація користувача через email та password
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            return Unauthorized(new { Message = "Invalid credentials" });
        }
        
      
      

        _logger.LogInformation("User {Email} logged in successfully", request.Email);
        var tokens = await _tokenService.GenerateTokensAsync(user);
        return Ok(tokens);
    }
    /// <summary>
    /// Оновлення access token через refresh token
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] TokenRequest request,
        [FromServices] UserManager<UserEntity> userManager,
        [FromServices] ITokenService tokenService)
    {
        var principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null) return BadRequest("Invalid access token");

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userManager.FindByIdAsync(userId!);

        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Unauthorized();

        var tokens = await _tokenService.GenerateTokensAsync(user);
       
        return Ok(tokens);
    }

    /// <summary>
    /// Реєстрація нового користувача
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> CreateUser([FromBody] RegistrationVM request )
    {
        _logger.LogInformation("Creating new user with email: {Email}", request.Email);

        if (request.Password != request.ConfirmPassword)
        {
            _logger.LogWarning("Password mismatch for user registration: {Email}", request.Email);
            return BadRequest("Password and Confirm Password do not match.");
        }

        var result = await _mediator.Send(new RegisterUserCommand(request));

        if (result.IsSuccess)
        {
            _logger.LogInformation("User created successfully: {Email}", request.Email);
            UserEntity? user = await _userManager.FindByEmailAsync(request.Email);
            if(user == null)
            {
                _logger.LogError("User not found after creation: {Email}", request.Email);
                return StatusCode(500, "User creation failed.");
            }
            TokenResponse tokens =  await _tokenService.GenerateTokensAsync(user);
            return Ok(tokens);
        }

        _logger.LogError("Failed to create user: {Email}. Errors: {Errors}", request.Email, result.Payload);
        return BadRequest(new { Message = "User creation failed", Errors = result.Payload });
    }

    /// <summary>
    /// Отримання користувача за ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(long id)
    {
        var result = await _mediator.Send(new GetUserQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Отримання профілю поточного користувача
    /// </summary>
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        return Ok(new { message = "Це видно лише з токеном" });
    }

    /// <summary>
    /// Отримання всіх користувачів
    /// </summary>
    [HttpGet]
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
    /// Перевірити чи email зареєстрований
    /// </summary>
    [AllowAnonymous]
    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmail([FromQuery] string email)
    {
        _logger.LogInformation("Check if email exists: {Email}", email);
        var user = await _userManager.FindByEmailAsync(email);
        return Ok(new { Exists = user != null });
    }

    /// <summary>
    /// Ініціація відновлення паролю
    /// - Генерує token і відправляє email з посиланням на фронтенд
    /// </summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        _logger.LogInformation("Forgot password requested for email: {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user != null)
        {
            // Rate-limiting: allow up to 5 requests per email per hour
            var cacheKey = $"forgot:{request.Email}";
            var attempts = _memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return 0;
            });

            if (attempts >= 5)
            {
                _logger.LogWarning("Too many forgot-password attempts for {Email}. Invalidating tokens.", request.Email);
                // Invalidate existing tokens by updating security stamp
                await _userManager.UpdateSecurityStampAsync(user);
                // Do not reveal details
                return StatusCode(429, new { Message = "Too many requests. Try again later." });
            }

            _memoryCache.Set(cacheKey, attempts + 1, TimeSpan.FromHours(1));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Build callback url for frontend. Prefer Origin header if present.
            var origin = Request.Headers["Origin"].FirstOrDefault() ?? $"{Request.Scheme}://{Request.Host}";
            var callbackUrl = $"{origin}/reset-password?email={WebUtility.UrlEncode(request.Email)}&token={WebUtility.UrlEncode(token)}";

            // Enqueue email to background worker (do not send on request thread)
            try
            {
                await _emailQueue.EnqueueEmailAsync(request.Email, callbackUrl);
                _logger.LogInformation("Enqueued password reset email for {Email}", request.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue password reset email for {Email}", request.Email);
            }
        }
        else
        {
            _logger.LogWarning("Forgot password requested for non-existent email: {Email}", request.Email);
        }

        // For now return a generic message so frontend can show a consistent UX
        return Ok(new { Message = "If the email exists, a password reset link will be sent." });
    }

    /// <summary>
    /// Test endpoint to enqueue a simple test email to any address (development use only).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("send-test-email")]
    public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { Message = "Email is required." });

        // Build a simple callback URL that will be inserted into the template (or shown in plain text)
        var origin = Request.Headers["Origin"].FirstOrDefault() ?? $"{Request.Scheme}://{Request.Host}";
        var callbackUrl = $"{origin}/test-email?to={WebUtility.UrlEncode(request.Email)}&id={Guid.NewGuid()}";

        try
        {
            await _emailQueue.EnqueueEmailAsync(request.Email, callbackUrl);
            _logger.LogInformation("Enqueued test email for {Email}", request.Email);
            return Ok(new { Message = "Test email enqueued." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue test email for {Email}", request.Email);
            return StatusCode(500, new { Message = "Failed to enqueue email." });
        }
    }

    public class TestEmailRequest
    {
        public string? Email { get;  }
    }

    /// <summary>
    /// Скидання паролю — застосовує token і новий пароль
    /// </summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        _logger.LogInformation("Reset password attempt for email: {Email}", request.Email);
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Reset password requested for unknown email: {Email}", request.Email);
            // do not reveal existence — return generic response
            return BadRequest(new { Message = "Invalid token or email." });
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to reset password for {Email}. Errors: {Errors}", request.Email, string.Join(',', result.Errors.Select(e => e.Description)));
            return BadRequest(new { Message = "Failed to reset password.", Errors = result.Errors.Select(e => e.Description) });
        }

        _logger.LogInformation("Password reset successful for {Email}", request.Email);
        return Ok(new { Message = "Password has been reset successfully." });
    }
}