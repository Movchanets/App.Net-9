using System.Security.Claims;
using Application.Commands.User.CreateUser;
using Application.Commands.User.Queries.GetUserByEmail;
using Application.Commands.User.Queries.GetUsers;
using Application.DTOs;
using Application.Interfaces;
using Application.Queries.User;
using AutoMapper;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<UserEntity> userManager;
    private readonly ITokenService tokenService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, UserManager<UserEntity> userManager, ITokenService tokenService, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        this.userManager = userManager;
        this.tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Тестовий endpoint для отримання токенів
    /// </summary>
    [AllowAnonymous]
    [HttpGet("test-tokens")]
    public async Task<IActionResult> GetTokensTEST()
    {
        var user = await userManager.FindByIdAsync("1");
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        return Ok(new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }
   
    /// <summary>
    /// Аутентифікація користувача через email та password
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);
        
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
            return Unauthorized(new { Message = "Invalid credentials" });
        }
        
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        // Збережемо refreshToken в БД або Redis
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);
        
        return Ok(new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
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

        var newAccessToken = tokenService.GenerateAccessToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        return Ok(new
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }

    /// <summary>
    /// Реєстрація нового користувача
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] RegisterUserCommand command)
    {
        _logger.LogInformation("Creating new user with email: {Email}", command.data.Email);
        
        if (command.data.Password != command.data.ConfirmPassword)
        {
            _logger.LogWarning("Password mismatch for user registration: {Email}", command.data.Email);
            return BadRequest("Password and Confirm Password do not match.");
        }
        
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("User created successfully: {Email}", command.data.Email);
        }
        else
        {
            _logger.LogError("Failed to create user: {Email}. Errors: {Errors}", command.data.Email, result.Payload);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Отримання користувача за ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
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
}