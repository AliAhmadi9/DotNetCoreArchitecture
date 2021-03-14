using Common;
using Common.Exceptions;
using Common.Utilities;
using Core.Data;
using Core.Data.Repositories;
using Core.Entities;
using ElmahCore.Mvc;
using ElmahCore.Sql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebFramework.Filters;

namespace WebFramework.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.EnableSensitiveDataLogging()
                       .UseSqlServer(configuration.GetConnectionString(/*"FrameworkDB"*/"OKRDataBase"), sqlServerOptionsAction =>
                       {
                           //sqlServerOptionsAction.CommandTimeout(60);
                           //sqlServerOptionsAction.EnableRetryOnFailure(3);
                       });
                //.UseLazyLoadingProxies(); Or to lazy loading enabled
                // to be add 'Microsoft.EntityFrameworkCore.Proxies' package
            });
        }

        readonly static string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public static void AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                                  builder =>
                                  {
                                      builder.WithOrigins("http://example.com", "http://test.com")
                                                          //.AllowAnyOrigin()
                                                          .AllowAnyHeader()
                                                          //.AllowAnyMethod()
                                                          ;
                                      builder.WithMethods("PUT", "DELETE", "GET", "POST");
                                  });
            });
        }

        public static void AddSessionStorage(this IServiceCollection services)
        {
            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(60);//You can set Time   
                                                               // You might want to only set the application cookies over a secure connection:
                                                               //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                                                               //options.Cookie.SameSite = SameSiteMode.Strict;
                                                               //options.Cookie.HttpOnly = true;
                                                               //// Make the session cookie essential
                options.Cookie.IsEssential = true;
            });
        }

        public static void AddElmah(this IServiceCollection services, IConfiguration configuration, SiteSettings siteSettings)
        {
            services.AddElmah<SqlErrorLog>(option =>
            {
                option.Path = siteSettings.ElmahPath;//default:/elmah
                option.ConnectionString = configuration.GetConnectionString("Elmah");
                //options.CheckPermissionAction = httpContext =>
                //{
                //    return httpContext.User.Identity.IsAuthenticated;
                //};
            });
        }

        public static void AddMinimalMvc(this IServiceCollection services)
        {
            #region AddControllers
            //This method configures the MVC services for the commonly used features with controllers for an API. 
            //This combines the effects of AddMvcCore(IServiceCollection), AddApiExplorer(IMvcCoreBuilder), AddAuthorization(IMvcCoreBuilder), 
            //AddCors(IMvcCoreBuilder), AddDataAnnotations(IMvcCoreBuilder), and AddFormatterMappings(IMvcCoreBuilder).
            #endregion

            #region AddControllersWithViews
            //This method configures the MVC services for the commonly used features with controllers with views.
            //This combines the effects of AddMvcCore(IServiceCollection), AddApiExplorer(IMvcCoreBuilder), AddAuthorization(IMvcCoreBuilder), 
            //AddCors(IMvcCoreBuilder), AddDataAnnotations(IMvcCoreBuilder), AddFormatterMappings(IMvcCoreBuilder), AddCacheTagHelper(IMvcCoreBuilder), 
            //AddViews(IMvcCoreBuilder), and AddRazorViewEngine(IMvcCoreBuilder).
            #endregion

            #region AddRazorPages
            //This method configures the MVC services for the commonly used features for pages.
            //This combines the effects of AddMvcCore(IServiceCollection), AddAuthorization(IMvcCoreBuilder), AddDataAnnotations(IMvcCoreBuilder), 
            //AddCacheTagHelper(IMvcCoreBuilder), and AddRazorPages(IMvcCoreBuilder).
            #endregion

            //###To add services for controllers with views call AddControllersWithViews(IServiceCollection) on the resulting builder.
            //###To add services for pages call AddRazorPages(IServiceCollection) on the resulting builder.
            //###To add services for controllers for APIs call AddControllers(IServiceCollection).

            services.AddMvcCore(options =>
            {
                options.Filters.Add(new AuthorizeFilter());

                //Like [ValidateAntiforgeryToken] attribute but dose not validatie for GET and HEAD http method
                //You can ingore validate by using [IgnoreAntiforgeryToken] attribute
                //Use this filter when use cookie 
                //options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

                //options.UseYeKeModelBinder();
            })
            .AddApiExplorer()
            .AddAuthorization()
            .AddFormatterMappings()
            .AddDataAnnotations()
            .AddViews()
            .AddRazorViewEngine()
            .AddMvcOptions(options =>
            {
                options.EnableEndpointRouting = false;
                //options.Filters.Add(new AuthorizeFilter());
            })
            .AddNewtonsoftJson(options =>
            {
                //options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                //options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            })
            //before core 3.1
            //.AddJsonFormatters(/*options =>
            //{
            //    options.Formatting = Newtonsoft.Json.Formatting.Indented;
            //    options.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            //}*/)
            .AddCors()
            .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddControllers(options =>
            {
                //var customAuthorize = new CustomAuthorize();
                //customAuthorize.AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
                //options.Filters.Add(customAuthorize);
            });
            //services.AddControllersWithViews();
            //services.AddRazorPages();
        }

        public static void AddJwtAthuntication(this IServiceCollection services, JwtSettings jwtSettings)
        {
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
            //or
            services.AddAuthentication(options =>
            {
                //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var secretkey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
                var encryptionkey = Encoding.UTF8.GetBytes(jwtSettings.Encryptkey);

                var validationParameters = new TokenValidationParameters
                {
                    //skew = انحراف
                    ClockSkew = TimeSpan.Zero, // default: 5 min --even 5 min after Expires and before NotBefore token is valid
                    RequireSignedTokens = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secretkey),

                    RequireExpirationTime = true,
                    ValidateLifetime = true,

                    ValidateAudience = true, //default : false
                    ValidAudience = jwtSettings.Audience,// jwtSettings.Audience,

                    ValidateIssuer = true, //default : false
                    ValidIssuer = jwtSettings.Issuer,// jwtSettings.Issuer

                    TokenDecryptionKey = new SymmetricSecurityKey(encryptionkey)
                };

                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = validationParameters;
                options.Events = new JwtBearerEvents
                {
                    //token exists in header but is invalid
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                        logger.LogError("Authentication failed!", context.Exception);

                        if (context.Exception != null)
                            throw new AppException(ApiResultStatusCode.UnAuthorized, "Authentication failed.", HttpStatusCode.Unauthorized, context.Exception, null);

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        #region test code
                        //var jwtSecurityToken = context.SecurityToken as JwtSecurityToken;
                        //ClaimsIdentity identity = context.Principal.Identity as ClaimsIdentity;
                        //var id = identity.GetUserId();

                        //var token = jwtSecurityToken.RawData;
                        //var handler = new JwtSecurityTokenHandler();
                        //var jsonToken = handler.ReadToken(token);
                        //var jwtSecurityToken2 = handler.ReadToken(token) as JwtSecurityToken;
                        #endregion

                        if (string.IsNullOrEmpty(CookieManager.Get(context.HttpContext, CookieManager.CookieKeys.JwtToken)))
                            context.Fail("OnAuthorization Method : Jwt Cookie is null!");

                        if (/*context.HttpContext.Request.IsAjax() &&*/
                        context.Principal.Identity.IsAuthenticated &&
                        ((System.IdentityModel.Tokens.Jwt.JwtSecurityToken)context.SecurityToken).RawData != CookieManager.Get(context.HttpContext, CookieManager.CookieKeys.JwtToken))
                        {
                            context.Fail("Jwt token in Request not valid(not equal to server token)!");
                        }

                        var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
                        var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

                        var claimsIdentity = context.Principal.Identity as ClaimsIdentity;

                        if (claimsIdentity.Claims?.Any() != true)
                            context.Fail("This token has no claims.");

                        var securityStamp = claimsIdentity.FindFirstValue(new ClaimsIdentityOptions().SecurityStampClaimType);
                        if (!securityStamp.HasValue())
                            context.Fail("This token has no secuirty stamp");

                        //Find user and token from database and perform your custom validation
                        var userId = new Guid(claimsIdentity.GetUserId());
                        var user = await userRepository.GetByIdAsync(context.HttpContext.RequestAborted, userId);

                        var securityStamp2 = await userManager.GetSecurityStampAsync(user);
                        var validatedUser = await signInManager.ValidateSecurityStampAsync(context.Principal);
                        if (validatedUser == null)
                            context.Fail("Token security stamp is not valid.");

                        if (!user.IsActive)
                            context.Fail("User is not active.");

                        await userRepository.UpdateLastLoginDateAsync(user, context.HttpContext.RequestAborted);
                    },
                    //token is not exists in header
                    OnChallenge = context =>
                    {
                        //var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                        //logger.LogError("OnChallenge error", context.Error, context.ErrorDescription);

                        if (context.AuthenticateFailure != null)
                            throw new AppException(ApiResultStatusCode.UnAuthorized, "Authenticate failure.", HttpStatusCode.Unauthorized, context.AuthenticateFailure, null);

                        throw new AppException(ApiResultStatusCode.UnAuthorized, "You are unauthorized to access this resource.", HttpStatusCode.Unauthorized);

                        //return Task.CompletedTask;
                    }
                };
            });
        }
        public static void AddCustomApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0); // v1.0 = v1
                //options.RouteConstraintName = "...";
                //options.ApiVersionReader = new UrlSegmentApiVersionReader();//default
                //options.ApiVersionReader = new MediaTypeApiVersionReader();
                //options.ApiVersionReader = new QueryStringApiVersionReader("api-version); // api/posts/api-version
                //options.ApiVersionReader = new HeaderApiVersionReader(new[] { "api-version-header" });                

                options.ReportApiVersions = true;
            });
        }


        public static string GetBulidNumber()
        {
            Version version = Assembly.GetEntryAssembly().GetName().Version;// Assembly.GetExecutingAssembly().GetName().Version;

            return $"{version.Major}{version.Minor}{version.Build}{version.Revision}";
        }
    }
}