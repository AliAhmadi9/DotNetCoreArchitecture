using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NLog.Web;
using Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using WebFramework.Api;
using WebFramework.Middlewares;

namespace Api.IntegrationTest
{
    public class TestFixture<TStartup> : IDisposable where TStartup : class
    {       
        public HttpClient Client { get; }
        private TestServer Server;

        public TestFixture()
            : this(Path.Combine(""))
        {
        }

        public void Dispose()
        {
            Client.Dispose();
            Server.Dispose();
        }

        protected TestFixture(string relativeTargetProjectParentDir)
        {
            #region comment code - using HostBuilder

            //var hostBuilder = new HostBuilder()
            //    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            //    .UseNLog()
            //    .ConfigureServices(services =>
            //    {
            //        services.AddAutofac();
            //        //.services.AddSingleton<IMyService, MyService>(); //Here
            //    })
            //    .ConfigureContainer<ContainerBuilder>(builder =>
            //    {
            //        //builder.RegisterType<MyService>().As<IMyService>().InstancePerLifetimeScope();//Here
            //    })
            //    .ConfigureWebHost(webHost =>
            //    {
            //        // Add TestServer
            //        webHost
            //       .UseStartup<TStartup>()
            //       .UseTestServer()
            //       .ConfigureServices(services =>
            //       {
            //            //services.AddSingleton<IMyService, MyService>();//Here
            //            services.AddAutofac();
            //            //services.AddControllers().AddApplicationPart(typeof(TestStartup).Assembly);
            //       })
            //       .ConfigureTestServices(services =>
            //       {
            //            //services.AddSingleton<IMyService, MyService>();//Here
            //            services.AddAutofac();
            //            //services.AddControllers().AddApplicationPart(typeof(TestStartup).Assembly);
            //       })
            //       .ConfigureTestContainer<ContainerBuilder>(builder1 =>
            //       {
            //           //builder.RegisterType<MyService>().As<IMyService>().InstancePerLifetimeScope();//Here
            //       });
            //    });

            //Server = hostBuilder.Start().GetTestServer();

            #endregion

            #region comment code - using WebHostBuilder

            //var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;
            //var contentRoot = GetProjectPath(relativeTargetProjectParentDir, startupAssembly);
            //var builder = new WebHostBuilder()
            //    .UseNLog()
            //    .ConfigureServices(services =>
            //    {
            //        //services.AddSingleton<IMyService, MyService>();//Here
            //        services.AddAutofac();
            //        //services.AddControllers().AddApplicationPart(typeof(TestStartup).Assembly);
            //    })
            //    //.ConfigureContainer<ContainerBuilder>(builder =>
            //    //{
            //    //    builder.RegisterType<MyService>().As<IMyService>().InstancePerLifetimeScope();//Here
            //    //})
            //    .ConfigureTestContainer<ContainerBuilder>(builder1 =>
            //    {
            //        //builder.RegisterType<MyService>().As<IMyService>().InstancePerLifetimeScope();//Here
            //    })
            //    .ConfigureTestServices(services =>
            //    {
            //        //services.AddSingleton<IMyService, MyService>();//Here
            //        services.AddAutofac();
            //        //services.AddControllers().AddApplicationPart(typeof(TestStartup).Assembly);
            //    })
            //    .UseContentRoot(contentRoot: $@"{contentRoot}")
            //    .UseConfiguration(new ConfigurationBuilder()
            //        .SetBasePath($@"{contentRoot}")
            //        .AddJsonFile("appsettings.development.json")
            //        .Build())
            //    .UseStartup<TStartup>();

            //Server = new TestServer(builder);

            #endregion

            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;
            var contentRoot = GetProjectPath(relativeTargetProjectParentDir, startupAssembly);

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile("appsettings.json");

            var webHostBuilder = new WebHostBuilder()
                .UseContentRoot(contentRoot)
                .UseNLog()
                .ConfigureServices(InitializeServices)
                //.ConfigureTestServices(services =>
                //{
                //    services.AddMvc(options =>
                //    {
                //        options.Filters.Add(new AllowAnonymousFilter());
                //        options.Filters.Add(new FakeUserFilter());
                //    })
                //    .AddApplicationPart(typeof(TStartup).Assembly);

                //    services.AddControllers(options =>
                //    {
                //        options.Filters.Add(new AllowAnonymousFilter());
                //        options.Filters.Add(new FakeUserFilter());
                //    })
                //    .AddApplicationPart(typeof(TStartup).Assembly);
                //})
                .UseConfiguration(configurationBuilder.Build())
                .UseEnvironment("Development")
                .Configure(configureApp => configureApp.UseCustomExceptionHandler())
                .UseStartup(typeof(TStartup));

            // Create instance of test server
            Server = new TestServer(webHostBuilder);

            // Add configuration for client
            Client = Server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost:5001/");
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected virtual void InitializeServices(IServiceCollection services)
        {
            services.AddAutofac();
            //services.AddMvc(options =>
            //{
            //    //options.Filters.Add(new AllowAnonymousFilter());
            //    options.Filters.Add(new FakeUserFilter());
            //})
            //    .AddApplicationPart(typeof(TStartup).Assembly);

            //services.AddControllers(options =>
            //{
            //    //options.Filters.Add(new AllowAnonymousFilter());
            //    options.Filters.Add(new FakeUserFilter());
            //})
            //    .AddApplicationPart(typeof(TStartup).Assembly);

            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;

            var manager = new ApplicationPartManager
            {
                ApplicationParts =
                {
                    new AssemblyPart(startupAssembly)
                },
                FeatureProviders =
                {
                    new ControllerFeatureProvider(),
                    new ViewComponentFeatureProvider()
                }
            };

            services.AddSingleton(manager);
        }

        public static string GetProjectPath(string projectRelativePath, Assembly startupAssembly)
        {
            var projectName = startupAssembly.GetName().Name;

            var applicationBasePath = AppContext.BaseDirectory;

            var directoryInfo = new DirectoryInfo(applicationBasePath);

            do
            {
                directoryInfo = directoryInfo.Parent;

                var projectDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, projectRelativePath));

                if (projectDirectoryInfo.Exists)
                    if (new FileInfo(Path.Combine(projectDirectoryInfo.FullName, projectName, $"{projectName}.csproj")).Exists)
                        return Path.Combine(projectDirectoryInfo.FullName, projectName);
            }
            while (directoryInfo.Parent != null);

            throw new Exception($"Project root could not be located using the application root {applicationBasePath}.");
        }        
    }

    public class FakeUserFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "e0b312ff-3a18-eb11-9661-000c2995ce1f"),
                new Claim(ClaimTypes.Name, "haseli"),
                new Claim(ClaimTypes.Email, "haseli2684@gmail.com"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Surname,"یدالله حاصل مهری"),
                new Claim(ClaimTypes.MobilePhone,"09126964896")
            }));

            await next();
        }
    }
}
