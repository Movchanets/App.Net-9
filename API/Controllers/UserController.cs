using System.Security.Claims;
using Application.Commands.User.CreateUser;
using Application.Commands.User.Queries.GetUserByEmail;
using Application.Commands.User.Queries.GetUsers;
using Application.DTOs;
using Application.Interfaces;
using Application.Queries.User;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request,
        [FromServices] UserManager<UserEntity> userManager,
        [FromServices] ITokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { Message = "Invalid credentials" });

        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!)
        };

        var accessToken = tokenService.GenerateAccessToken(userClaims);
        var refreshToken = tokenService.GenerateRefreshToken();

        // Збережемо refreshToken в БД або Redis
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        return Ok(new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }
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

        var newAccessToken = tokenService.GenerateAccessToken(principal.Claims);
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

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] RegisterUserCommand command)
    {
        if (command.data.Password != command.data.ConfirmPassword)
        {
            return BadRequest("Password and Confirm Password do not match.");
        }
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _mediator.Send(new GetUserQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }
    [HttpGet()]
    public async Task<IActionResult> GetUsers()
    {
        var result = await _mediator.Send(new GetUsersQuery());
        if (result.Payload == null) return NotFound();
        return Ok(result);
    }
    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var result = await _mediator.Send(new GetUserByEmailQuery(email));
        return Ok(result);
    }
}