using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Services.Dto;
using Services.Model;

namespace Services
{
    public interface IUserService
    {
        Task<string> Token(LoginModel model, CancellationToken cancellationToken = default);

        Task<string> GeneratePasswordResetToken(ForgotPasswordModel model, CancellationToken cancellationToken = default);

        Task<IdentityResult> ResetPassword(ResetPasswordModel model, CancellationToken cancellationToken = default);

        Task<List<UserSelectDto>> Get(CancellationToken cancellationToken = default);

        Task<UserSelectDto> Get(Guid id, CancellationToken cancellationToken = default);

        Task<User> Create(UserDto userDto, CancellationToken cancellationToken = default);

        Task Update(UserDto userDto, CancellationToken cancellationToken = default);

        Task Delete(Guid id, CancellationToken cancellationToken = default);

        Task<UserSelectDto> UpdateCustomProperties(UserDto userDto, CancellationToken cancellationToken = default);

        Task UpdateFullname(UserDto user, CancellationToken cancellationToken=default);
    }
}