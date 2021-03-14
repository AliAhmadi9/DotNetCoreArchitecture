using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using Common.Exceptions;
using Core.Data;
using Core.Data.Repositories;
using Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Dto;
using Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class UserService : IUserService, IScopedDependency
    {
        private readonly IMapper mapper;
        private readonly IUserRepository repository;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly RoleManager<Role> roleManager;
        private readonly IJwtService jwtService;
        private readonly ILogger<IUserRepository> logger;

        public UserService(IMapper mapper, IUserRepository repository, UserManager<User> userManager, SignInManager<User> signInManager,
                           RoleManager<Role> roleManager, IJwtService jwtService, ILogger<IUserRepository> logger)
        {
            this.mapper = mapper;
            this.repository = repository;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.jwtService = jwtService;
            this.logger = logger;
        }

        public virtual async Task<string> Token(LoginModel model, CancellationToken cancellationToken = default)
        {
            //var user = await userRepository.GetByUserAndPass(username, password, cancellationToken);
            var user = await userManager.FindByEmailAsync(model.Email);

            //find by username
            //  user = await userManager.FindByNameAsync(model.Email);
            if (user == null)
                throw new BadRequestException("نام کاربری یا کلمه عبور صحیح نمی باشد!");

            var isPasswordValid = await userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
                throw new BadRequestException("نام کاربری یا کلمه عبور صحیح نمی باشد!");

            var token = await jwtService.GenerateAsync(user);

            #region Login

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await signInManager.PasswordSignInAsync(user.UserName, model.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                logger.LogInformation("User logged in.");
        
                return token;
            }
            if (result.RequiresTwoFactor)
            {
                //return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                throw new AppException("حساب کاربری شما غیر فعال شده است!");
            }
            else
            {
                throw new AppException("خطایی نامشخص رخ داده است!");
            }

            #endregion
        }

        public virtual async Task<string> GeneratePasswordResetToken(ForgotPasswordModel model, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new BadRequestException("حساب کاربری با ایمیل وارد شده یافت نشد!");

            //if (!await userManager.IsEmailConfirmedAsync(user))
            //    throw new BadRequestException("ایمیل تایید نشده است!!");

            // For more information on how to enable account confirmation and password reset please 
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            return code;
        }

        public virtual async Task<IdentityResult> ResetPassword(ResetPasswordModel model, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new BadRequestException("حساب کاربری یا ایمیل وارد شده یافت نشد!");

            var decodedCode = WebEncoders.Base64UrlDecode(model.Code);
            string decoded = Encoding.UTF8.GetString(decodedCode, 0, decodedCode.Length);
            return await userManager.ResetPasswordAsync(user, decoded, model.Password);
        }

        public virtual async Task<List<UserSelectDto>> Get(CancellationToken cancellationToken = default)
        {
            var users = await repository.TableNoTracking
                .Include(p => p.UserRoles)
                    .ThenInclude(p => p.Role)
                .ProjectTo<UserSelectDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return users;
        }

        public virtual async Task<UserSelectDto> Get(Guid id, CancellationToken cancellationToken)
        {
            //var user = await userManager.FindByIdAsync(id.ToString());
            var user = await repository.TableNoTracking
                .Include(p => p.UserRoles)
                    .ThenInclude(p => p.Role)
                .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (user == null)
                return null;

            var userDto = mapper.Map<UserSelectDto>(user);

            return userDto;
        }

        public virtual async Task<User> Create(UserDto userDto, CancellationToken cancellationToken = default)
        {
            var user = mapper.Map<User>(userDto);
            var result = await userManager.CreateAsync(user, userDto.Password);
            if (result.Succeeded)
            {
                #region Add User to Role(s)
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    result = await roleManager.CreateAsync(new Role() { Name = "Admin", Description = "Admin" });
                    if (result.Succeeded)
                    {
                        if ((await userManager.AddToRolesAsync(user, new[] { "Admin" })).Succeeded)
                            return user;
                    }
                }
                else
                {
                    if ((await userManager.AddToRolesAsync(user, new[] { "Admin" })).Succeeded)
                        return user;
                }
                #endregion
                logger.LogInformation("User created a new account with password.");
            }

            throw new AppException(string.Join("<br/>", result.Errors.Select(p => p.Description)));
        }

        public async Task Update(UserDto userDto, CancellationToken cancellationToken = default)
        {
            if (userDto == null)
                throw new NotFoundException("'userDto' can't be NULL");

            var user = await repository.GetByIdAsync(cancellationToken, userDto.Id);
            if (user == null)
                throw new NotFoundException($"not found User entity to PKey(Id) : '{userDto.Id}'");

            var updateUser = mapper.Map(userDto, user);

            await repository.UpdateAsync(updateUser, cancellationToken);
        }

        public virtual async Task Delete(Guid id, CancellationToken cancellationToken = default)
        {
            await repository.DeleteByIdAsync(id, cancellationToken);
        }

        public async Task UpdateFullname(UserDto userDto, CancellationToken cancellationToken = default)
        {
            if (userDto == null)
                throw new NotFoundException("'userDto' can not NULL");

            var user = await repository.GetByIdAsync(cancellationToken, userDto.Id);
            if (user == null)
                throw new NotFoundException($"not found User entity to PKey(Id) : '{userDto.Id}'");

            await repository.UpdateCustomPropertiesAsync(user, cancellationToken, true, new string[] { nameof(User.FullName) });
        }

        public async Task<UserSelectDto> UpdateCustomProperties(UserDto userDto, CancellationToken cancellationToken = default)
        {
            if (userDto == null)
                throw new NotFoundException("'dto' can not NULL");

            if (userDto.UpdateProperties == null || !userDto.UpdateProperties.Any())
                throw new NotFoundException("'dto.UpdateProperties' can't be NULL or Empty");

            var model = await repository.TableNoTracking.SingleOrDefaultAsync(p => p.Id == userDto.Id, cancellationToken);
            //var model = await userManager.FindByIdAsync(userDto.Id.ToString());
            if (model == null)
                throw new NotFoundException($"not found User entity to PKey(Id) : '{userDto.Id}'");

            var concurrencyStamp = model.GetType().GetProperty("ConcurrencyStamp").GetValue(model).ToString();
            model = mapper.Map<User>(userDto);
            model.GetType().GetProperty("ConcurrencyStamp").SetValue(model, concurrencyStamp);

            await repository.UpdateCustomPropertiesAsync(model, cancellationToken, true, userDto.UpdateProperties.ToArray());

            var resultDto = await repository.TableNoTracking.ProjectTo<UserSelectDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(p => p.Id.Equals(model.Id), cancellationToken);

            return resultDto;
        }
    }
}
