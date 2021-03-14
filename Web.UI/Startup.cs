using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using ElmahCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebEssentials.AspNetCore.Pwa;
using WebFramework.Configuration;
using WebFramework.CustomMapping;
using WebFramework.Middlewares;

namespace Web.UI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public ILifetimeScope AutofacContainer { get; private set; }

        private readonly SiteSettings siteSettings;
        private readonly SecretSiteSettings secretSiteSettings;

        public Startup(IWebHostEnvironment env)
        {
            // In ASP.NET Core 3.0 `env` will be an IWebHostEnvironment, not IHostingEnvironment.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            this.Configuration = builder.Build();

            siteSettings = Configuration.GetSection(nameof(SiteSettings)).Get<SiteSettings>();
            secretSiteSettings = Configuration.GetSection(nameof(SecretSiteSettings)).Get<SecretSiteSettings>();
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutofac();

            // Add services to the collection. Don't build or return
            // any IServiceProvider or the ConfigureContainer method
            // won't get called. Don't create a ContainerBuilder
            // for Autofac here, and don't call builder.Populate() - that
            // happens in the AutofacServiceProviderFactory for you.
            services.AddOptions();

            //... normal registration here
            services.Configure<SiteSettings>(Configuration.GetSection(nameof(SiteSettings)));

            services.AddDbContext(Configuration);

            //Identity Settings(Microsoft Identity Core)
            services.AddCustomIdentity(siteSettings.IdentitySettings);

            //services.AddGoogleAuthentication(secretSiteSettings.ExternalAuthentication.GoogleAuth);

            services.AddElmah(Configuration, siteSettings);

            services.AddDistributedMemoryCache();

            services.AddSessionStorage();

            services.AddCorsPolicy();

            services.AddMinimalMvc();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddProgressiveWebApp(new PwaOptions
            {
                CacheId = $"Worker {WebFramework.Configuration.ServiceCollectionExtensions.GetBulidNumber()}",
                Strategy = ServiceWorkerStrategy.NetworkFirst,
                AllowHttp = true,
                //RoutesToPreCache = "https://localhost:44351/home, https://localhost:44351//project/index",
                OfflineRoute = "/offline.html",
            });

            services.InitializeAutoMapper();

            services.AddCustomApiVersioning();

            services.AddJwtAthuntication(siteSettings.JwtSettings);

            services.BuildAutofacServiseProvider();
        }

        #region comment to unit test bug in identity
        //just Core 3.0+ to above
        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you. If you
        // need a reference to the container, you need to use the
        // "Without ConfigureContainer" mechanism shown later.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Register your own things directly with Autofac here. Don't
            // call builder.Populate(), that happens in AutofacServiceProviderFactory
            // for you.
            // Register your own things directly with Autofac
            builder.AddServices();
        }
        #endregion

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // If, for some reason, you need a reference to the built container, you
            // can use the convenience extension method GetAutofacRoot.
            //this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            //app.UseHttpsRedirection();
            app.IntializeDatabase();

            app.UseCustomExceptionHandler();

            app.UseHttpsRedirection();

            //must be before => app.UseMvc();
            app.UseSession();

            app.UseCookiePolicy();

            //must be before => app.UseMvc();
            app.UseCustomStaticFiles();

            app.UseHsts(env);

            //default url to middleware : http://localhost/elmah
            app.UseElmah();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();//must be before => app.UseMvc();
            app.UseAuthorization();

            //create middleware to check jwt token
            //app.UseJwtMiddlewareHandler();

            //UseEndpoints
            app.UseCustomeRouting();

            app.UseMvc();
        }
    }
}
