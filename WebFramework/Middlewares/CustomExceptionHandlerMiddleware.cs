using Common;
using Common.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebFramework.Api;

namespace WebFramework.Middlewares
{
    public static class CustomExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
        }
    }
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

        public CustomExceptionHandlerMiddleware(RequestDelegate next,
            IWebHostEnvironment env,
            ILogger<CustomExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            string message = null;
            var httpStatusCode = HttpStatusCode.InternalServerError;
            var apiStatusCode = ApiResultStatusCode.ServerError;

            try
            {
                await _next(context);
            }
            catch (AppException exception)
            {
                _logger.LogError(exception, exception.Message);
                httpStatusCode = exception.HttpStatusCode;
                apiStatusCode = exception.ApiStatusCode;

                if (context.Request.IsAjax() || (_env.IsDevelopment() && context.Request.Headers.Keys.Contains("Postman-Token")))
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
                        context.Response.Redirect(context.User.Identity.IsAuthenticated ? "/Home" : "/Login");
                    }
                }
                ThrowExceptionIfRequestIsTest(exception);
            }
            catch (SecurityTokenExpiredException exception)
            {
                _logger.LogError(exception, exception.Message);
                if (context.Request.IsAjax() || (_env.IsDevelopment() && context.Request.Headers.Keys.Contains("Postman-Token")))
                {
                    SetUnAuthorizeResponse(exception);
                    await WriteToResponseAsync();
                }
                else
                {
                    if (httpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        context.Response.Redirect(context.User.Identity.IsAuthenticated ? "/Home" : "/Login");
                    }
                }
                ThrowExceptionIfRequestIsTest(exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                _logger.LogError(exception, exception.Message);
                if (context.Request.IsAjax() || (_env.IsDevelopment() && context.Request.Headers.Keys.Contains("Postman-Token")))
                {
                    SetUnAuthorizeResponse(exception);
                    await WriteToResponseAsync();
                }
                else
                {
                    if (httpStatusCode == HttpStatusCode.Unauthorized)
                    {
                        context.Response.Redirect(context.User.Identity.IsAuthenticated ? "/Home" : "/Login");
                    }
                }

                ThrowExceptionIfRequestIsTest(exception);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                if (context.Request.IsAjax() || (_env.IsDevelopment() && context.Request.Headers.Keys.Contains("Postman-Token")))
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
                        context.Response.Redirect(context.User.Identity.IsAuthenticated ? "/Home" : "/Login");
                    }
                }

                ThrowExceptionIfRequestIsTest(exception);
            }

            void ThrowExceptionIfRequestIsTest(Exception exp)
            {
                if (context.Request.Host.HasValue && context.Request.Host.Port == 5001 && context.Request.Host.Host.Equals("localhost"))
                    throw new Exception(exp.Message, exp);
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

                if (_env.IsDevelopment() || (_env.IsDevelopment() && context.Request.Headers.Keys.Contains("Postman-Token")))
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