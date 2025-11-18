using System.Security.Claims;
using Infrastructure.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Infrastructure.Services;

public class ClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<ApplicationUser>(userManager, optionsAccessor)
{
    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user); // claims from Identity (roles, etc.)
        var identity = (ClaimsIdentity)principal.Identity;

        var domain = user.DomainUser;
        if (domain != null)
        {
            if (!string.IsNullOrEmpty(domain.Name))
            {
                identity.AddClaim(new Claim(ClaimTypes.GivenName, domain.Name));
            }
            if (!string.IsNullOrEmpty(domain.Surname))
            {
                identity.AddClaim(new Claim(ClaimTypes.Surname, domain.Surname));
            }
            if (domain.Avatar != null)
            {
                identity.AddClaim(new Claim(JwtRegisteredClaimNames.Picture, domain.Avatar.StorageKey));
            }
        }
        return principal;
    }
}
