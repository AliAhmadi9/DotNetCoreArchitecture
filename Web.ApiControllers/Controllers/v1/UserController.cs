using AutoMapper;
using Common;
using Common.Exceptions;
using Common.Utilities;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Services;
using Services.Dto;
using Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Common.Utilities.Email;
using Core.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using WebFramework.Api;
using WebFramework.Configuration;
using WebFramework.Filters;

namespace Web.ApiControllers.Controllers.v1
{
    [ApiVersion("1")]
    [AllowAnonymous]
    public class UserController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<User> _signInManager;
        private readonly IRepository<User> _userRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        //private readonly ILogger<IUserRepository> logger;
        //private readonly IHttpContextAccessor httpContextAccessor;

        public UserController(IMapper mapper, IUserService userService, UserManager<User> userManager,
            IEmailSender emailSender, SignInManager<User> signInManager, IRepository<User> userRepository, IWebHostEnvironment webHostEnvironment)
        {
            _mapper = mapper;
            _userService = userService;
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _userRepository = userRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public virtual async Task<string> Token(LoginModel model, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByConditionAsync(u => u.Email == model.Email, cancellationToken);

            if (!user.EmailConfirmed)
                throw new AppException("حساب شما فعال نیست ، لطفا ابتدا حساب خود را فعال کنید !");

            var token = await _userService.Token(model, cancellationToken);
            CookieManager.Set(HttpContext, CookieManager.CookieKeys.JwtToken, token, CookieManager.ExpireTimeMode.Day, 14);

            return token;
        }

        [HttpPost("[action]")]
        //[AllowAnonymous]
        public virtual async Task<string> ForgotPassword(ForgotPasswordModel model, CancellationToken cancellationToken, bool isFirstTime = false)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new BadRequestException("حساب کاربری با ایمیل وارد شده یافت نشد!");

            var code = await _userService.GeneratePasswordResetToken(model, cancellationToken);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            //var callbackUrl = Url.RouteUrl(
            //    "ResetPassword",
            //    new { code, email = user.Email },
            //    Request.Scheme);

            var callbackUrl = Url.Action("ResetPassword", "Account",
                new { code, email = user.Email }, HttpContext.Request.Scheme);


            if (!isFirstTime)
            {
                await _emailSender.SendEmailAsync(new MailRequest
                {
                    ToEmail = user.Email,
                    Subject = "بازیابی رمزعبور",
                    Body = await EmailCreator.Create(new EmailCreatorModel
                    {
                        BodyText = "برای تغییر رمز عبور خود بر روی لینک زیر کلیک کنید",
                        Link = HtmlEncoder.Default.Encode(callbackUrl),
                        Name = user.FullName
                    }, _webHostEnvironment.WebRootPath, cancellationToken)
                });
            }
            else
            {
                await _emailSender.SendEmailAsync(new MailRequest
                {
                    ToEmail = user.Email,
                    Subject = "هدف سنج",
                    Body = await EmailCreator.Create(new EmailCreatorModel
                    {
                        BodyText = "شما به یک محیط کاری در هدف سنج اضافه شده اید . برای ادامه روی لینک زیر کلیک کنید.",
                        Link = HtmlEncoder.Default.Encode(callbackUrl),
                        Name = "سلام کاربر عزیز"
                    }, _webHostEnvironment.WebRootPath, cancellationToken)
                });
            }

            return "لطفا ایمیل خود را چک کنید،جهت تغییر کلمه عبور،ایمیلی برای شما ارسال شده است";
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public virtual async Task<string> ResetPassword(ResetPasswordModel model, CancellationToken cancellationToken)
        {
            var result = await _userService.ResetPassword(model, cancellationToken);

            if (result.Succeeded)
                return "تغییر کلمه عبور با موفقیت انجام شد";

            throw new AppException(string.Join("<br/>", result.Errors.Select(p => p.Description)));
        }

        [HttpPost("[action]")]
        [CustomAuthorize]
        public async Task<bool> SetNewPassword(SetNewPasswordModel model, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(cancellationToken, new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier)));

            if (!await _userManager.CheckPasswordAsync(user, model.OldPassword))
                throw new AppException("رمزعبور قدیمی اشتباه است !");

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.Password);

            return result.Succeeded;
        }

        [HttpGet]
        //[CustomAuthorize("Admin", "User", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public virtual async Task<ActionResult<List<UserDto>>> Get(CancellationToken cancellationToken)
        {
            var users = await _userService.Get(cancellationToken);

            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        //[CustomAuthorize("Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public virtual async Task<ApiResult<UserSelectDto>> Get(Guid id, CancellationToken cancellationToken)
        {
            var userSelectDto = await _userService.Get(id, cancellationToken);

            if (userSelectDto == null)
                return NotFound();

            return userSelectDto;
        }

        [HttpPost]
        [AllowAnonymous]
        //[CustomAuthorize("Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public virtual async Task<ApiResult<UserSelectDto>> Create(UserDto userDto, CancellationToken cancellationToken, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var newUser = await _userService.Create(userDto, cancellationToken);
            if (newUser != null)
            {
                await SendEmail(new ResendEmailModel
                {
                    Email = newUser.Email
                }, cancellationToken);

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    //todo not complete this code

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        null,
                        new { area = "Identity", userId = newUser.Id, code, returnUrl },
                        Request.Scheme);

                    await _emailSender.SendEmailAsync(new MailRequest
                    {
                        ToEmail = userDto.Email,
                        Subject = "Confirm your email",
                        Body = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>."
                    });
                    //return RedirectToPage("RegisterConfirmation", new { email = userDto.Email, returnUrl = returnUrl });

                    //todo not complete this code

                }
                else
                {
                    //var token = await  jwtService.GenerateAsync(newUser);
                    var token = await _userService.Token(new LoginModel { Email = userDto.Email, Password = userDto.Password }, cancellationToken);
                    CookieManager.Set(HttpContext, CookieManager.CookieKeys.JwtToken, token, CookieManager.ExpireTimeMode.Day, 14);
                    await _signInManager.SignInAsync(newUser, false);
                    var userSelectDto = _mapper.Map<UserSelectDto>(newUser);
                    userSelectDto.ReturnUrl = returnUrl;
                    return userSelectDto;
                }
            }
            return null;
        }

        [HttpPut]
        //[CustomAuthorize("Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public virtual async Task<ApiResult> Update(UserDto userDto, CancellationToken cancellationToken)
        {
            await _userService.Update(userDto, cancellationToken);

            return Ok();
        }

        //[CustomAuthorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("{id:guid}")]
        public virtual async Task<ApiResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _userService.Delete(id, cancellationToken);

            return Ok();
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<UserSelectDto> UpdateFullname(dynamic dynamicDto, CancellationToken cancellationToken)
        {
            UserDto userDto = UtilConvertor.ToObject<UserDto>(dynamicDto);
            List<string> properties = UtilConvertor.GetPropertiesName(dynamicDto);
            userDto.UpdateProperties = properties;
            var userSelectDto = await _userService.UpdateCustomProperties(userDto, cancellationToken);

            return userSelectDto;
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<ApiResult<string>> SendEmail(ResendEmailModel model, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                throw new BadRequestException("حساب کاربری با ایمیل وارد شده یافت نشد!");


            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action("ConfirmEmail", "Account", new
            {
                userId = user.Id,
                token = code
            }, HttpContext.Request.Scheme);

            var encodedLink = HtmlEncoder.Default.Encode(callbackUrl);

            await _emailSender.SendEmailAsync(new MailRequest
            {
                ToEmail = model.Email,
                Subject = "تایید ایمیل هدف سنج",
                Body = await EmailCreator.Create(new EmailCreatorModel
                {
                    BodyText = "برای تایید ایمیل خودتون بر روی دکمه زیر کلیک کنید",
                    Link = encodedLink,
                    Name = $"{user.Email.Split('@')[0]} عزیز ، سلام"
                }, _webHostEnvironment.WebRootPath, cancellationToken)
            });

            return encodedLink;
        }
    }
}
