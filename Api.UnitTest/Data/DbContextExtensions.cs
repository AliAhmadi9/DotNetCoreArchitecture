using Core.Data;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Api.UnitTest.Data
{
    public static class DbContextExtensions
    {
        public static void Seed(this ApplicationDbContext dbContext)
        {
            //#region Category

            //dbContext.Add(new Category()
            //{
            //    Id = 1,
            //    Name = "دسته بندی 1"
            //});

            //dbContext.Add(new Category()
            //{
            //    Id = 2,
            //    Name = "دسته بندی 2"
            //});

            //dbContext.Add(new Category()
            //{
            //    Id = 3,
            //    Name = "دسته بندی 3",
            //    ParentCategoryId = 1

            //});

            //dbContext.Add(new Category()
            //{
            //    Id = 4,
            //    Name = "دسته بندی 4",
            //    ParentCategoryId = 2

            //});

            //#endregion

            //#region Post

            //dbContext.Add(new Post()
            //{
            //    Id=new Guid("428eede8-860c-eb11-9661-000c2995ce1f"),
            //    Capacity = 25,
            //    CategoryId = 1,
            //    Description = "توضیحات ندارد",
            //    IsActive = true,
            //    Price = 250000,
            //    Title = "محصول شماره 1"
            //});

            //dbContext.Add(new Post()
            //{
            //    Id = new Guid("428eede8-860c-eb11-9661-000c2995ce1e"),
            //    Capacity = 45,
            //    CategoryId = 2,
            //    Description = "توضیحات ندارد",
            //    IsActive = true,
            //    Price = 450000,
            //    Title = "محصول شماره 2"
            //});

            //dbContext.Add(new Post()
            //{
            //    Id = new Guid("428eede8-860c-eb11-9661-000c2995ce1d"),
            //    Capacity = 85,
            //    CategoryId = 3,
            //    Description = "توضیحات ندارد",
            //    IsActive = true,
            //    Price = 550000,
            //    Title = "محصول شماره 3"
            //});

            //dbContext.Add(new Post()
            //{
            //    Id = new Guid("428eede8-860c-eb11-9661-000c2995ce1a"),
            //    Capacity = 21,
            //    CategoryId = 3,
            //    Description = "توضیحات ندارد",
            //    IsActive = true,
            //    Price = 620000,
            //    Title = "محصول شماره 4"
            //});

            //#endregion

            dbContext.SaveChanges();
        }
    }
}
