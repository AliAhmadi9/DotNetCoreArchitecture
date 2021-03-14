using Api.UnitTest.Data;
using AutoMapper;
using Core.Data;
using Core.Data.Repositories;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using Services;
using Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Api.UnitTest.Controllers.ApiControllers.v1
{
    public class UserControllerUnitTest : IClassFixture<ApplicationDbFixture>
    {
        readonly ApplicationDbFixture fixture;
        public UserControllerUnitTest(ApplicationDbFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestGetAsync()
        {
            try
            {
                //Arrange
                var users = new List<User>
                {
                    new User
                    {
                        UserName = "Test",
                        Id = Guid.NewGuid(),
                        Email = "test@test.it",
                        EmailConfirmed=true,
                        PhoneNumberConfirmed=true,
                        UserRoles = new List<UserRole>
                        {
                            new UserRole()
                            {
                                Role = new Role { Name="Admin" },
                            },
                            new UserRole()
                            {
                                Role = new Role { Name="Manager" },
                            }
                        }
                    }
                }.AsQueryable();

                var mapper = (IMapper)fixture.Server.Host.Services.GetService(typeof(IMapper));
                var jwtSevice = (IJwtService)fixture.Server.Host.Services.GetService(typeof(IJwtService));
                var emailSender = (IEmailSender)fixture.Server.Host.Services.GetService(typeof(IEmailSender)); 
                var logger = (ILogger<IUserRepository>)fixture.Server.Host.Services.GetService(typeof(ILogger<IUserRepository>));
                var dbContext2 = (ApplicationDbContext)fixture.Server.Host.Services.GetService(typeof(ApplicationDbContext));
                var userRepository2 = (IUserRepository)fixture.Server.Host.Services.GetService(typeof(IUserRepository));
                var mapper2 = AutoMapperSingleton.Mapper;
                var fakeUserManager = new Mock<FakeUserManager>();
                var fakeSignInManager = new Mock<FakeSignInManager>();

                fakeUserManager.Setup(x => x.Users).Returns(users);
                fakeUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                    .ReturnsAsync(It.IsAny<User>());
                var user = await fakeUserManager.Object.FindByNameAsync("Test");
                var users2 = fakeUserManager.Object.Users.ToList();

                var dbContext =  fixture.dbContext;// DbContextMocker.GetDbContext(nameof(CategoriesControllerUnitTest));
                var userRepository = new Repository<User>(dbContext);
                //var userService = new UserService(AutoMapperSingleton.Mapper, userRepository, fakeUserManager, fakeSignInManager, logger);
                //var controller = new UserController(AutoMapperSingleton.Mapper, userService);

                ////Act
                //var result = await controller.Get(default);

                ////dbContext.Dispose();

                ////Assert
                //if (result != null && result.IsSuccess)
                //    Assert.True(true, $"Post Count : {result.Data.Count}");
                //else
                Assert.True(true);

            }
            catch (Exception exp)
            {
                Assert.False(true, exp.Message);
            }

        }

        //[Fact]
        //public async Task TestGetPostAsync()
        //{
        //    try
        //    {
        //        //Arrange
        //        var dbContext = fixture.dbContext;// DbContextMocker.GetDbContext(nameof(CategoriesControllerUnitTest));
        //        var postRepository = new Repository<Post>(dbContext);
        //        var postService = new ServiceRepository<PostDto, PostSelectDto, Post, Guid>(AutoMapperSingleton.Mapper, postRepository);
        //        var controller = new PostsController(AutoMapperSingleton.Mapper, postService);

        //        //Act
        //        var result = await controller.Get(id: new Guid("428eede8-860c-eb11-9661-000c2995ce1a"), default);

        //        //dbContext.Dispose();

        //        //Assert
        //        if (result != null && result.IsSuccess)
        //            Assert.True(true, $"Post FullTitle : {result.Data.FullTitle}");
        //        else
        //            Assert.True(false);

        //    }
        //    catch (Exception exp)
        //    {
        //        Assert.False(true, exp.Message);
        //    }

        //}

        //[Fact]
        //public async Task TestSelectAsync()
        //{
        //    try
        //    {
        //        //Arrange
        //        var dbContext = fixture.dbContext;// DbContextMocker.GetDbContext(nameof(CategoriesControllerUnitTest));
        //        var postRepository = new Repository<Post>(dbContext);
        //        var postService = new ServiceRepository<PostDto, PostSelectDto, Post, Guid>(AutoMapperSingleton.Mapper, postRepository);
        //        var controller = new PostsController(AutoMapperSingleton.Mapper, postService);

        //        //Act
        //        var result = await controller.Select(pageNumber: 1, pageSize: 10, default);

        //        //dbContext.Dispose();

        //        //Assert
        //        if (result != null && result.IsSuccess)
        //            Assert.True(true, $"TotalRowCount : {result.Data.TotalRowCount} , TotalPages : {result.Data.TotalPages}");
        //        else
        //            Assert.True(false);

        //    }
        //    catch (Exception exp)
        //    {
        //        Assert.False(true, exp.Message);
        //    }

        //}
    }
}
