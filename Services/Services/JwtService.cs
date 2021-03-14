using Core.Entities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Common;
using System.Threading.Tasks;

namespace Services
{
    public class JwtService : IJwtService, IScopedDependency
    {
        private readonly SiteSettings siteSettings;
        private readonly SignInManager<User> signInManager;

        public JwtService(IOptionsSnapshot<SiteSettings> siteSetting, SignInManager<User> signInManager)
        {
            siteSettings = siteSetting.Value;
            this.signInManager = signInManager;
        }
        public async Task<string> GenerateAsync(User user)
        {
            var securityKey = Encoding.UTF8.GetBytes(siteSettings.JwtSettings.SecretKey);//longer than 16 character
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(securityKey), SecurityAlgorithms.HmacSha256Signature);

            var encryptionkey = Encoding.UTF8.GetBytes(siteSettings.JwtSettings.Encryptkey); //must be 16 character
            var encryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(encryptionkey), SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);

            var claims = await getClaimsAsync(user);
            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = siteSettings.JwtSettings.Issuer,
                Audience = siteSettings.JwtSettings.Audience,
                IssuedAt = DateTime.Now,
                //NotBefore = DateTime.Now.AddMinutes(5),
                NotBefore = DateTime.Now.AddMinutes(0),
                Expires = DateTime.Now.AddDays(14),
                SigningCredentials = signingCredentials,
                EncryptingCredentials = encryptingCredentials,
                Subject = new ClaimsIdentity(claims)
            };

            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(descriptor);

            var jwt = tokenHandler.WriteToken(securityToken);

            return jwt;
        }

        private async Task<IEnumerable<Claim>> getClaimsAsync(User user)
        {
            var claims = (await signInManager.ClaimsFactory.CreateAsync(user)).Claims;
            //add custom claims
            var claimsList = new List<Claim>(claims);
            //claimsList.Add(new Claim(ClaimTypes.MobilePhone, "09123456987"));

            return claimsList;

            #region custom authentications
            ////JwtRegisteredClaimNames.Sub
            //var securityStampClaimType = new ClaimsIdentityOptions().SecurityStampClaimType;

            //var list = new List<Claim>
            //{
            //    new Claim(ClaimTypes.Name, user.UserName),
            //    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            //    new Claim(ClaimTypes.MobilePhone, "09123456987"),
            //    new Claim("FullName",user.FullName),
            //    new Claim(securityStampClaimType, user.SecurityStamp.ToString())
            //};

            //var roles = new Role[]
            //{
            //    new Role { Name = "Admin" },
            //    new Role { Name = "SuperAdmin" }
            //};
            //foreach (var role in roles)
            //    list.Add(new Claim(ClaimTypes.Role, role.Name));

            //return list;
            #endregion
        }
    }
}
