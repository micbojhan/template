using System;
using System.Linq;
using Company.WebApplication1.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Company.WebApplication1.Infrastructure.DataAccess.Data.Seed
{
    public static class ApplicationDbContextExtensions
    {
        public static void MigrateAndSeedData(this ApplicationDbContext context, IServiceProvider provider)
        {
            // check that all migrations have been applied
            if (context.Database.GetPendingMigrations().Any())
            {
                // if there are pendnig migrations, run them
                context.Database.Migrate();
            }

#if (IndividualAuth)
            if (!context.Users.Any(x => x.Email == "test@test.com"))
            {
                var userToInsert = new ApplicationUser { Email = "test@test.com", UserName = "test@test.com", PhoneNumber = "12345678" };
                var password = "Password@123";

                // locator anti-pattern - TODO find a better way
                var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
                userManager.CreateAsync(userToInsert, password).Wait();
            }
#endif
        }
    }
}
