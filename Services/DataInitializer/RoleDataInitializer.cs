using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Services.DataInitializer
{
    public class RoleDataInitializer : DataInitializer, IDataInitializer
    {
        private readonly RoleManager<Role> roleManager;

        public RoleDataInitializer(RoleManager<Role> roleManager)
        {
            this.roleManager = roleManager;
        }

        public void InitializeData()
        {
            if (!roleManager.Roles.AsNoTracking().Any(p => p.Name == "Admin"))
            {
                var role = new Role
                {
                    Name = "Admin",
                    Description = "Admin Role"
                };
                var result = roleManager.CreateAsync(role).GetAwaiter().GetResult();
            }
        }
    }
}