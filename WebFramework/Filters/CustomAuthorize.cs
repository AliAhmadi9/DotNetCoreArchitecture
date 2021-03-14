using Common;
using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using WebFramework.Api;
using WebFramework.Configuration;

namespace WebFramework.Filters
{
    //public class ClaimRequirementAttribute : TypeFilterAttribute
    //{
    //    public ClaimRequirementAttribute(int claimType) : base(typeof(CustomAuthorization))
    //    {
    //        Arguments = new object[] { claimType };
    //    }
    //    public ClaimRequirementAttribute(string claimType, string claimValue) : base(typeof(CustomAuthorization))
    //    {
    //        Arguments = new object[] { new Claim(claimType, claimValue) };
    //    }
    //}


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomAuthorize : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string[] roles;
        
        public CustomAuthorize(params string[] roles)
        {
            this.roles = roles;
            if (!string.IsNullOrEmpty(Roles))
            {
                foreach (var role in Roles.Split(','))
                {
                    this.roles.Append(role);
                }
            }
        }

        /// <summary>  
        /// This will Authorize User  
        /// </summary>  
        /// <returns></returns>  
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext != null)
            {
                if (filterContext.Filters.Any(p => p is Microsoft.AspNetCore.Mvc.Authorization.AllowAnonymousFilter))
                    return;

                //var logger = (ILogger<CustomAuthorize>)filterContext.HttpContext.RequestServices.GetService(typeof(ILogger<CustomAuthorize>));
                var logger = filterContext.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ILogger<CustomAuthorize>));
                if (string.IsNullOrEmpty(CookieManager.Get(filterContext.HttpContext, CookieManager.CookieKeys.JwtToken)))
                {
                    if (filterContext.HttpContext.Request.IsAjax())
                    {
                        //handle in ServiceCollectionExtensions->AddJwtAthuntication->OnTokenValidated

                        //throw new AppException(ApiResultStatusCode.UnAuthorized,
                        //                       "OnAuthorization Method :Not Authenticate,You are unauthorized to access this resource.",
                        //                       HttpStatusCode.Unauthorized);
                    }
                    else
                    {
                        logger.LogError("OnAuthorization Method : Jwt Session is null!");
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "Login" }));
                    }
                }
                if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    if (roles.Length > 0)
                    {
                        var claimsIdentity = filterContext.HttpContext.User.Identity as ClaimsIdentity;
                        if (claimsIdentity.Claims?.Any() == true)
                        {
                            var userRoles = claimsIdentity.GetRoles();
                            if (userRoles == null || !userRoles.Any(role => roles.Contains(role)))
                            {
                                if (filterContext.HttpContext.Request.IsAjax())
                                {
                                    throw new AppException(ApiResultStatusCode.UnAuthorized,
                                                           "OnAuthorization Method : You are unauthorized to access this resource.",
                                                           HttpStatusCode.Unauthorized);
                                }
                                else
                                {
                                    logger.LogError("OnAuthorization Method : You are unauthorized to access this resource.");
                                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "Login" }));
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (filterContext.HttpContext.Request.IsAjax())
                    {
                        throw new AppException(ApiResultStatusCode.UnAuthorized,
                                               "OnAuthorization Method :Not Authenticate,You are unauthorized to access this resource.",
                                               HttpStatusCode.Unauthorized);
                    }
                    else
                    {
                        logger.LogError("OnAuthorization Method : You are unauthorized to access this resource.");
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "Login" }));
                    }
                }

            }
        }
    }
}
