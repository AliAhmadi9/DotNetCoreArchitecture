using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using WebFramework.Configuration;

namespace Web.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //add to unit test
            CreateHostBuilder(args)
                //.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                //.ConfigureServices(services => services.AddAutofac())
                //.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                //.ConfigureContainer<ContainerBuilder>(builder =>
                //{
                //    // Register your own things directly with Autofac
                //    builder.AddServices();
                //})
                //.UseConsoleLifetime()
                .Build()
                .Run();
            //add to unit test

            //CreateHostBuilder(args)
            //    .Build()
            //    .Run();

            //Set deafult proxy
            //WebRequest.DefaultWebProxy = new WebProxy("http://127.0.0.1:8118", true) { UseDefaultCredentials = true };

            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
           .ConfigureLogging(option => option.ClearProviders())
           .UseNLog()//service inject to loggers
           .ConfigureAppConfiguration((hostContext, builder) =>
           {
               // Add other providers for JSON, etc.
               if (hostContext.HostingEnvironment.IsDevelopment())
               {
                   //builder.AddUserSecrets<Program>();
               }
           })
           .ConfigureWebHostDefaults(webBuilder =>
           {
               webBuilder.UseStartup<Startup>();
               webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
               webBuilder.UseIISIntegration();
           })
           .UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }
}
