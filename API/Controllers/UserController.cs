using System.Security.Claims;
using Application.Commands.User.CreateUser;
using System.Net;
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
using Application.Commands.User.AuthenticateUser;
using Application.Commands.User.ForgotPassword;
using Application.Commands.User.RefreshTokenCommand;
using Application.Commands.User.ResetPassword;
using Application.Commands.User.SendTestEmail;
using Application.Queries.User.CheckEmail;
// email services moved into Application layer; controller uses MediatR commands
using Microsoft.AspNetCore.Authorization;
using API.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace API.Controllers;

[Authorize]
[ServiceFilter(typeof(TurnstileValidationFilter))]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<UserEntity> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, UserManager<UserEntity> userManager, ITokenService tokenService, IMemoryCache memoryCache, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        this._userManager = userManager;
        this._tokenService = tokenService;
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

        try
        {
            var tokens = await _mediator.Send(new AuthenticateUserCommand(request));
            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            return Ok(tokens);
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            return Unauthorized(new { Message = "Invalid credentials" });
        }
    }
    /// <summary>
    /// Оновлення access token через refresh token
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] TokenRequest request)
    {
        try
        {
            var tokens = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken));
            return Ok(tokens);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    /// <summary>
    /// Реєстрація нового користувача
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> CreateUser([FromBody] RegistrationVM request)
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
            if (user == null)
            {
                _logger.LogError("User not found after creation: {Email}", request.Email);
                return StatusCode(500, "User creation failed.");
            }
            TokenResponse tokens = await _tokenService.GenerateTokensAsync(user);
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
    public async Task<IActionResult> CheckEmail([FromQuery] string email, [FromQuery] string? turnstileToken)
    {
        _logger.LogInformation("Check if email exists: {Email}", email);
        // Turnstile token (if present) will be validated by TurnstileValidationFilter
        var result = await _mediator.Send(new CheckEmailQuery(email));
        return Ok(result);
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
        var origin = Request.Headers["Origin"].FirstOrDefault() ?? $"{Request.Scheme}://{Request.Host}";
        try
        {
            await _mediator.Send(new ForgotPasswordCommand(request.Email, origin, request.TurnstileToken));
            return Ok(new { Message = "If the email exists, a password reset link will be sent." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed processing forgot-password for {Email}", request.Email);
            return StatusCode(500, new { Message = "Failed to process request." });
        }
    }

    /// <summary>
    /// Test endpoint to enqueue a simple test email to any address (development use only).
    /// </summary>
    [AllowAnonymous]
    [HttpGet("send-test-email/{email}")]
    public async Task<IActionResult> SendTestEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { Message = "Email is required." });
        var origin = Request.Headers["Origin"].FirstOrDefault() ?? $"{Request.Scheme}://{Request.Host}";
        try
        {
            await _mediator.Send(new SendTestEmailCommand(email, origin));
            _logger.LogInformation("Enqueued test email for {Email}", email);
            return Ok(new { Message = "Test email enqueued." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue test email for {Email}", email);
            return StatusCode(500, new { Message = "Failed to enqueue email." });
        }
    }

    /// <summary>
    /// Скидання паролю — застосовує token і новий пароль
    /// </summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        _logger.LogInformation("Reset password attempt for email: {Email}", request.Email);
        try
        {
            await _mediator.Send(new ResetPasswordCommand(request));
            _logger.LogInformation("Password reset successful for {Email}", request.Email);
            return Ok(new { Message = "Password has been reset successfully." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to reset password for {Email}. Error: {Error}", request.Email, ex.Message);
            return BadRequest(new { Message = "Failed to reset password.", Errors = new[] { ex.Message } });
        }
    }
}