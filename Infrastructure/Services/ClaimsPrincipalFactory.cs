using System.Security.Claims;
using Application.Interfaces;
using Infrastructure.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Infrastructure.Services;

public class ClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    IOptions<IdentityOptions> optionsAccessor,
    IFileStorage fileStorage)
    : UserClaimsPrincipalFactory<ApplicationUser>(userManager, optionsAccessor)
{
    private readonly IFileStorage _fileStorage = fileStorage;
    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user); // claims from Identity (roles, etc.)
        var identity = (ClaimsIdentity?)principal.Identity;
        if (identity == null) return principal;

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
            if (domain.Avatar != null && !string.IsNullOrWhiteSpace(domain.Avatar.StorageKey))
            {
                var avatarUrl = _fileStorage.GetPublicUrl(domain.Avatar.StorageKey);
                identity.AddClaim(new Claim("avatarUrl", avatarUrl));
            }
        }
        return principal;
    }
}
