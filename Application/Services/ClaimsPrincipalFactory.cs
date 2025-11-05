using System.Security.Claims;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Application.Services;

public class ClaimsPrincipalFactory(
    UserManager<UserEntity> userManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<UserEntity>(userManager, optionsAccessor)
{
    public override async Task<ClaimsPrincipal> CreateAsync(UserEntity user)
    {
        var principal = await base.CreateAsync(user); // Отримує всі claims з БД (включаючи "urn:myapp:storeid")
        var identity = (ClaimsIdentity)principal.Identity;

        // Динамічно додати FirstName та LastName, якщо вони існують
        if (!string.IsNullOrEmpty(user.Name))
        {
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.Name));
        }

        if (!string.IsNullOrEmpty(user.Surname))
        {
            identity.AddClaim(new Claim(ClaimTypes.Surname, user.Surname));
        }
        if(!string.IsNullOrEmpty(user.ImageUrl))
        {
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Picture, user.ImageUrl));
        }
        return principal;
    }
}