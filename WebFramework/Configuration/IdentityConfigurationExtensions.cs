using Common;
using Core.Data;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace WebFramework.Configuration
{
    public static class IdentityConfigurationExtensions
    {
        public static void AddCustomIdentity(this IServiceCollection services, IdentitySettings settings)
        {
            services.AddIdentity<User, Role>(identityOptions =>
             {
                 //Password Settings
                 identityOptions.Password.RequireDigit = settings.PasswordRequireDigit;
                 identityOptions.Password.RequiredLength = settings.PasswordRequiredLength;
                 identityOptions.Password.RequireNonAlphanumeric = settings.PasswordRequireNonAlphanumic; //#@!
                 identityOptions.Password.RequireUppercase = settings.PasswordRequireUppercase;
                 identityOptions.Password.RequireLowercase = settings.PasswordRequireLowercase;

                 //UserName Settings
                 identityOptions.User.RequireUniqueEmail = settings.RequireUniqueEmail;

                 //Singin Settings
                 //identityOptions.SignIn.RequireConfirmedEmail = false;
                 //identityOptions.SignIn.RequireConfirmedPhoneNumber = false;

                 //Lockout Settings
                 //identityOptions.Lockout.MaxFailedAccessAttempts = 5;
                 //identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                 //identityOptions.Lockout.AllowedForNewUsers = false;
             })
             .AddEntityFrameworkStores<ApplicationDbContext>()
             .AddDefaultTokenProviders();

            // https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-3.1#globally-require-all-users-to-be-authenticated
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
                options.Cookie.Name = "KarSaatIdentityCookie";
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
            });
        }

        public static void AddGoogleAuthentication(this IServiceCollection services, GoogleAuth googleAuth )
        {
            services.AddAuthentication(
            //    options =>
            //{
            //    options.DefaultScheme = "Application";
            //    options.DefaultSignInScheme = "External";
            //}
            )
                //.AddCookie("Application")
                //.AddCookie("External")
                .AddGoogle("google", opt =>
                {
                    opt.ClientId = googleAuth.ClientId;
                    opt.ClientSecret = googleAuth.ClientSecret;
                    opt.SignInScheme = IdentityConstants.ExternalScheme;
                });
        }
    }
}
