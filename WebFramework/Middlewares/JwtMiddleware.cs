using Common;
using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WebFramework.Api;
using WebFramework.Configuration;

namespace WebFramework.Middlewares
{
    public static class JwtMiddlewareMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtMiddlewareHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtMiddleware>();
        }
    }
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next,
            IWebHostEnvironment env,
            ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            string message = null;
            HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError;
            ApiResultStatusCode apiStatusCode = ApiResultStatusCode.ServerError;

            try
            {
                var isAjax = context.Request.IsAjax();
                var isPostAjax = context.Request.IsAjax("POST");
                var isSentFromPostMan = context.Request.Headers.ContainsKey("Postman-Token");
                if (isSentFromPostMan)
                {
                    var postManValue = context.Request.Headers["Postman-Token"];
                }

                if (isAjax && context.User.Identity.IsAuthenticated &&
                    !string.IsNullOrEmpty(CookieManager.Get(context, CookieManager.CookieKeys.JwtToken)) &&
                    !string.IsNullOrEmpty(context.Request.Headers["Authorization"]) &&
                    context.Request.Headers["Authorization"] != $"Bearer {CookieManager.Get(context, CookieManager.CookieKeys.JwtToken)}")
                {
                    throw new BadRequestException("Jwt token in Request not valid!");
                }

                await _next(context);
            }
            catch (AppException exception)
            {
                _logger.LogError(exception, exception.Message);
                httpStatusCode = exception.HttpStatusCode;
                apiStatusCode = exception.ApiStatusCode;

                if (context.Request.IsAjax())
                {
                    if (_env.IsDevelopment())
                    {
                        var dic = new Dictionary<string, string>
                        {
                            ["Exception"] = exception.Message,
                            ["StackTrace"] = exception.StackTrace,
                        };
                        if (exception.InnerException != null)
                        {
                            dic.Add("InnerException.Exception", exception.InnerException.Message);
                            dic.Add("InnerException.StackTrace", exception.InnerException.StackTrace);
                        }
                        if (exception.AdditionalData != null)
                            dic.Add("AdditionalData", JsonConvert.SerializeObject(exception.AdditionalData));

                        message = JsonConvert.SerializeObject(dic);
                    }
                    else
                    {
                        message = exception.Message;
                    }
                    await WriteToResponseAsync();
                }
                else
                {
                    if (httpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (context.User.Identity.IsAuthenticated)
                            context.Response.Redirect("/Home");
                        else
                            context.Response.Redirect("/Login");
                    }
                }
            }
            catch (SecurityTokenExpiredException exception)
            {
                _logger.LogError(exception, exception.Message);
                if (context.Request.IsAjax())
                {
                    SetUnAuthorizeResponse(exception);
                    await WriteToResponseAsync();
                }
                else
                {
                    if (httpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (context.User.Identity.IsAuthenticated)
                            context.Response.Redirect("/Home");
                        else
                            context.Response.Redirect("/Login");
                    }
                }
            }
            catch (UnauthorizedAccessException exception)
            {
                _logger.LogError(exception, exception.Message);
                if (context.Request.IsAjax())
                {
                    SetUnAuthorizeResponse(exception);
                    await WriteToResponseAsync();
                }
                else
                {
                    if (httpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (context.User.Identity.IsAuthenticated)
                            context.Response.Redirect("/Home");
                        else
                            context.Response.Redirect("/Login");
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                if (context.Request.IsAjax())
                {
                    if (_env.IsDevelopment())
                    {
                        var dic = new Dictionary<string, string>
                        {
                            ["Exception"] = exception.Message,
                            ["StackTrace"] = exception.StackTrace,
                        };
                        message = JsonConvert.SerializeObject(dic);
                    }
                    await WriteToResponseAsync();
                }
                else
                {
                    if (httpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        if (context.User.Identity.IsAuthenticated)
                            context.Response.Redirect("/Home");
                        else
                            context.Response.Redirect("/Login");
                    }
                }
            }

            async Task WriteToResponseAsync()
            {
                if (context.Response.HasStarted)
                    throw new InvalidOperationException("The response has already started, the http status code middleware will not be executed.");

                var result = new ApiResult(false, apiStatusCode, message);
                var json = JsonConvert.SerializeObject(result);

                context.Response.StatusCode = (int)httpStatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(json);
            }

            void SetUnAuthorizeResponse(Exception exception)
            {
                httpStatusCode = HttpStatusCode.Unauthorized;
                apiStatusCode = ApiResultStatusCode.UnAuthorized;

                if (_env.IsDevelopment())
                {
                    var dic = new Dictionary<string, string>
                    {
                        ["Exception"] = exception.Message,
                        ["StackTrace"] = exception.StackTrace
                    };
                    if (exception is SecurityTokenExpiredException tokenException)
                        dic.Add("Expires", tokenException.Expires.ToString());

                    message = JsonConvert.SerializeObject(dic);
                }
            }
        }
    }
}
