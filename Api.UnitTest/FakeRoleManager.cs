using Core.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Api.UnitTest
{
    //public class FakeRoleManager : RoleManager<User>
    //{
    //    public FakeRoleManager()
    //        : base(new Mock<IRoleStore<Role>>().Object,
    //              new Mock<IRoleValidator<User>>,
    //              new HttpContextAccessor(),
    //              new Mock<IUserClaimsPrincipalFactory<User>>().Object,
    //              new Mock<IOptions<IdentityOptions>>().Object,
    //              new Mock<ILogger<SignInManager<User>>>().Object,
    //              new Mock<IAuthenticationSchemeProvider>().Object,
    //              new Mock<IUserConfirmation<User>>().Object)
    //    { }
    //}
}
