using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Exceptions;
using Core.Data.Repositories;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.Dto;
using Services.Model;
using WebFramework.Api;
using WebFramework.Configuration;
using WebFramework.Filters;

namespace Web.UI.Controllers
{
    public class AccountController : BaseMvcController
    {
        private readonly IMapper _mapper;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IJwtService _jwtService;
        public AccountController(SignInManager<User> signInManager, IMapper mapper, UserManager<User> userManager, IJwtService jwtService)
        {
            _signInManager = signInManager;
            _mapper = mapper;
            _userManager = userManager;
            _jwtService = jwtService;
        }


        [AllowAnonymous]
        public async Task<ActionResult<LoginModel>> Login(string returnUrl = null)
        {
            var model = new LoginModel
            {
                ReturnUrl = returnUrl ?? Url.Action("Index", "Home")
            };

            if (User.Identity.IsAuthenticated && string.IsNullOrEmpty(CookieManager.Get(HttpContext, CookieManager.CookieKeys.JwtToken)))
                await _signInManager.SignOutAsync();
            else if (User.Identity.IsAuthenticated)
                return Redirect(model.ReturnUrl);

            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            return View(model);
        }

        //#region External Logins
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[AllowAnonymous]
        //public ActionResult ExternalLogin(string provider, string returnUrl = null)
        //{
        //    var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        //    var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        //    return Challenge(properties, provider);
        //}

        //[AllowAnonymous]
        //public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        //{
        //    returnUrl ??= Url.Action("Index", "Home");
        //    var info = await _signInManager.GetExternalLoginInfoAsync();
        //    if (info == null)
        //    {
        //        return RedirectToAction(nameof(Login));
        //    }
        //    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        //    var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        //    if (signInResult.Succeeded)
        //    {
        //        var user = await _userManager.FindByEmailAsync(email);
        //        if (user != null)
        //        {
        //            #region generate jwt token & save in cookie
        //            var token = await _jwtService.GenerateAsync(user);
        //            CookieManager.Set(HttpContext, CookieManager.CookieKeys.JwtToken, token, CookieManager.ExpireTimeMode.Day, 14);
        //            #endregion
        //        }

        //        return LocalRedirect(returnUrl);
        //    }
        //    if (signInResult.IsLockedOut)
        //    {
        //        return RedirectToRoute("forgotpassword");
        //    }
        //    else
        //    {
        //        return View("ExternalLogin", new ExternalLoginModel { Email = email, ProviderDisplayName = info.LoginProvider, FullName = info.Principal.Identity.Name, ReturnUrl = returnUrl });
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[AllowAnonymous]
        //public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginModel model, CancellationToken cancellationToken, string returnUrl = null)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    var info = await _signInManager.GetExternalLoginInfoAsync();
        //    if (info == null)
        //    {
        //        //Error loading external login information during confirmation.
        //        return RedirectToAction(nameof(Login));
        //    }
        //    var user = await _userManager.FindByEmailAsync(model.Email);
        //    IdentityResult result;
        //    if (user != null)
        //    {
        //        result = await _userManager.AddLoginAsync(user, info);
        //        if (result.Succeeded)
        //        {
        //            #region generate jwt token & save in cookie
        //            var token = await _jwtService.GenerateAsync(user);
        //            CookieManager.Set(HttpContext, CookieManager.CookieKeys.JwtToken, token, CookieManager.ExpireTimeMode.Day, 14);
        //            #endregion

        //            await _signInManager.SignInAsync(user, isPersistent: false);
        //            return LocalRedirect(returnUrl);
        //        }
        //    }
        //    else
        //    {
        //        model.Principal = info.Principal;
        //        user = new User { Email = model.Email, UserName = model.Email, FullName = model.FullName };
        //        result = await _userManager.CreateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            //ino bayad beporsam ke che konam
        //            //??????????
        //            //Workspace workspace = new Workspace
        //            //{ Name = "فضای کاری پیش فرض" };

        //            //await workSpaceRepository.AddAsync(workspace, new CancellationToken());

        //            ////create Membership
        //            //Membership membership = new Membership
        //            //{ UserId = user.Id, WorkspaceId = workspace.Id };

        //            //await membershipRepository.AddAsync(membership, new CancellationToken());
        //            ////set default usermembershipId
        //            //user.DefaultWorkspace = workspace.Id;
        //            //await _userManager.UpdateAsync(user);

        //            //  CookieManager.Set(HttpContext, CookieManager.CookieKeys.CurrentWorkSpace, workspace.Id.ToString());

        //            result = await _userManager.AddLoginAsync(user, info);
        //            if (result.Succeeded)
        //            {
        //                result = await _userManager.AddToRolesAsync(user, new[] { "Admin" });
        //                if (result.Succeeded)
        //                {
        //                }
        //                #region generate jwt token & save in cookie
        //                var token = await _jwtService.GenerateAsync(user);
        //                CookieManager.Set(HttpContext, CookieManager.CookieKeys.JwtToken, token, CookieManager.ExpireTimeMode.Day, 14);
        //                #endregion

        //                //TODO: Send an email for the email confirmation and add a default role as in the Register action
        //                await _signInManager.SignInAsync(user, isPersistent: false);
        //                return LocalRedirect(returnUrl);
        //            }
        //        }
        //    }

        //    foreach (var error in result.Errors)
        //    {
        //        ModelState.TryAddModelError(error.Code, error.Description);
        //    }
        //    return View(nameof(ExternalLogin), model);
        //}
        //#endregion

        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> Register(string returnUrl = null)
        {
            var model = new UserDto
            {
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };

            if (User.Identity.IsAuthenticated && string.IsNullOrEmpty(CookieManager.Get(HttpContext, CookieManager.CookieKeys.JwtToken)))
                await _signInManager.SignOutAsync();
            else if (User.Identity.IsAuthenticated)
                return Redirect(model.ReturnUrl);

            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult<ForgotPasswordModel>> ForgotPassword()
        {
            var model = new ForgotPasswordModel();

            if (User.Identity.IsAuthenticated && string.IsNullOrEmpty(CookieManager.Get(HttpContext, CookieManager.CookieKeys.JwtToken)))
                await _signInManager.SignOutAsync();

            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult<ResendEmailModel>> ResendEmailConfirmation()
        {
            if (User.Identity.IsAuthenticated && string.IsNullOrEmpty(CookieManager.Get(HttpContext, CookieManager.CookieKeys.JwtToken)))
                await _signInManager.SignOutAsync();

            return View(new ResendEmailModel());
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return BadRequest();

            var user = _signInManager.UserManager.FindByIdAsync(userId).Result;

            var decodedToken = Encoding.Default.GetString(WebEncoders.Base64UrlDecode(token));

            var result = await _signInManager.UserManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded || _signInManager.UserManager.IsEmailConfirmedAsync(user).Result)
                return RedirectToAction("Login", "Account");

            user.EmailConfirmed = true;

            await _signInManager.UserManager.UpdateAsync(user);

            return View();
        }

        [AllowAnonymous]
        public IActionResult Confirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult<ResetPasswordModel>> ResetPassword(string email, string code = null)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(email))
                throw new BadRequestException("برای تغییر کلمه عبور کد لازم است!");

            var model = new ResetPasswordModel
            {
                Email = email,
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
            };

            if (User.Identity.IsAuthenticated ||
                string.IsNullOrEmpty(CookieManager.Get(HttpContext, CookieManager.CookieKeys.JwtToken)))
            {
                CookieManager.RemoveAllCookie(HttpContext);
                await _signInManager.SignOutAsync();
            }

            return View(model);
        }


        [CustomAuthorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            CookieManager.RemoveAllCookie(HttpContext);

            return RedirectToAction("Login");
        }
    }
}