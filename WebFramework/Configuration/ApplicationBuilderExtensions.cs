using Common.Utilities;
using Core.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.DataInitializer;
using System.Linq;

namespace WebFramework.Configuration
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseHsts(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            Assert.NotNull(app, nameof(app));
            Assert.NotNull(env, nameof(env));

            if (!env.IsDevelopment())
                app.UseHsts();
        }

        public static void UseCustomeRouting(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {                
                endpoints.MapControllers();

                endpoints.MapControllerRoute(
                   name: "login",
                   pattern: "login",
                   defaults: new { controller = "Account", action = "Login" });

                //  endpoints.MapControllerRoute(
                //name: "signin-google",
                //pattern: "signin-google",
                //defaults: new { controller = "Account", action = "LoginCallback" });
                endpoints.MapControllerRoute(
                    name: "register",
                    pattern: "register",
                    defaults: new { controller = "User", action = "Register" });

                endpoints.MapControllerRoute(
                    name: "forgotpassword",
                    pattern: "forgotpassword",
                    defaults: new { controller = "User", action = "ForgotPassword" });

                endpoints.MapControllerRoute(
                    name: "resetpassword",
                    pattern: "resetpassword",
                    defaults: new { controller = "User", action = "ResetPassword" });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                //endpoints.MapDefaultControllerRoute();


                #region Sample Routes
                //endpoints.MapControllerRoute(
                //  name: "searchCustomer",
                //  pattern: "{area:exists}/customer/search/page{page}",
                //  defaults: new { area = "admin", controller = "Customer", action = "Search" },
                //  constraints: new { page = new IntRouteConstraint() });

                //endpoints.MapControllerRoute(
                //    name: "searchCustomer_get",
                //    pattern: "{area:exists}/customer/{id?}",
                //    defaults: new { area = "admin", controller = "Customer", action = "Index" },
                //    constraints: new { id = new IntRouteConstraint() });

                //endpoints.MapControllerRoute(
                //    name: "importCustomer",
                //    pattern: "{area:exists}/customer/importcustomer/page{page}",
                //    defaults: new { area = "admin", controller = "Customer", action = "ImportCustomer" },
                //    constraints: new { page = new IntRouteConstraint() });

                //endpoints.MapControllerRoute(
                //    name: "importAccounts",
                //    pattern: "{area:exists}/customer/importaccounts/campaing{campaingId}.page{page}.tr{totalRowCount}.tp{totalPages}",
                //    defaults: new { area = "admin", controller = "Customer", action = "ImportAccounts" },
                //    constraints: new
                //    {
                //        campaingId = new IntRouteConstraint(),
                //        page = new IntRouteConstraint(),
                //        totalRowCount = new IntRouteConstraint(),
                //        totalPages = new IntRouteConstraint()
                //    });

                //endpoints.MapControllerRoute(
                //    name: "importAccounts2",
                //    pattern: "{area:exists}/customer/importaccounts/campaing{campaingId}.page{page}",
                //    defaults: new { area = "admin", controller = "Customer", action = "ImportAccounts" },
                //    constraints: new
                //    {
                //        campaingId = new IntRouteConstraint(),
                //        page = new IntRouteConstraint()
                //    });

                //endpoints.MapControllerRoute(
                //   name: "ImportCustomerFinantial",
                //   pattern: "{area:exists}/customer/importcustomerfinantial/{date}/campaing{campaingId}.page{page}.tr{totalRowCount}.tp{totalPages}",
                //   defaults: new { area = "admin", controller = "Customer", action = "ImportCustomerFinantial" },
                //   constraints: new
                //   {
                //       campaingId = new IntRouteConstraint(),
                //       date = new IntRouteConstraint(),
                //       page = new IntRouteConstraint(),
                //       totalRowCount = new IntRouteConstraint(),
                //       totalPages = new IntRouteConstraint()
                //   });

                //endpoints.MapControllerRoute(
                //    name: "ImportCustomerFinantial2",
                //    pattern: "{area:exists}/customer/importcustomerfinantial/{date}/campaing{campaingId}.page{page}",
                //    defaults: new { area = "admin", controller = "Customer", action = "ImportCustomerFinantial" },
                //    constraints: new
                //    {
                //        campaingId = new IntRouteConstraint(),
                //        page = new IntRouteConstraint(),
                //        date = new IntRouteConstraint()
                //    });


                //endpoints.MapControllerRoute(
                //    name: "contract",
                //    pattern: "{area:exists}/Contract/{customerNo?}",
                //     defaults: new { action = "Index" },
                //       constraints: new { currentPage = new IntRouteConstraint() });

                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{controller=Home}/{action=Index}/{id?}");
                #endregion
            });
        }

        public static void UseCustomStaticFiles(this IApplicationBuilder app)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    var url = ctx.Context.Request.Path.ToString();
                    var filePath = ctx.File.PhysicalPath;
                    if (!ctx.Context.User.Identity.IsAuthenticated)
                    {
                        // respond HTTP 401 Unauthorized.
                        //ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    }
                }
            });
        }

        public static void IntializeDatabase(this IApplicationBuilder app)
        {
            //Use C# 8 using variables
            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>(); //Service locator

            //Dos not use Migrations, just Create Database with latest changes
            //dbContext.Database.EnsureCreated();
            //Applies any pending migrations for the context to the database like (Update-Database)
            dbContext.Database.Migrate();

            var dataInitializers = scope.ServiceProvider.GetServices<IDataInitializer>().OrderBy(p => p.Order);
            foreach (var dataInitializer in dataInitializers)
                dataInitializer.InitializeData();
        }
    }
}
