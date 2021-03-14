using Common;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebFramework.Configuration
{
    public class AdditionalUserClaimsPrincipalFactory
        : UserClaimsPrincipalFactory<User>, IScopedDependency
    {
        public AdditionalUserClaimsPrincipalFactory(
            UserManager<User> userManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        { }

        public async override Task<ClaimsPrincipal> CreateAsync(User user)
        {
            var principal = await base.CreateAsync(user);
            var identity = (ClaimsIdentity)principal.Identity;
            var userRoles = await UserManager.GetRolesAsync(user);

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Surname, user.FullName ?? string.Empty));
            foreach (var role in userRoles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            identity.AddClaims(claims);
            return principal;
        }
    }
}
