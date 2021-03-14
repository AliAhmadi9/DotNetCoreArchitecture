using Autofac;
using Autofac.Extensions.DependencyInjection;
using Core.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

//https://stackoverflow.com/questions/59275542/net-core-3-0-service-not-register-using-hostbuilder-for-integration-tests
namespace Api.UnitTest.Data
{
    public class ApplicationDbFixture : IDisposable
    {
        //public readonly ApplicationDbContext dbContext;
        public ApplicationDbContext dbContext { get; private set; }
        public readonly TestServer Server;

        public ApplicationDbFixture() : this(Path.Combine(""))
        {

        }
        protected ApplicationDbFixture(string relativeTargetProjectParentDir)
        {
            // Create options for DbContext instance
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"MemoryDatabase_{Guid.NewGuid()}")
                .Options;
            dbContext = new ApplicationDbContext(options);

            //if (dbContext != null)
            //{
            //    dbContext.Database.EnsureDeleted();
            //    dbContext.Database.EnsureCreated();
            //}

            dbContext.Seed();
        }

        public void Dispose()
        {
            //dbContext.Database.EnsureDeleted();
            dbContext.DisposeAsync();
        }
    }
}
