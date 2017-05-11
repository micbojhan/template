using System.Linq;
using Company.WebApplication1.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Company.WebApplication1.Infrastructure.DataAccess.Data.Seed
{
    public static class ApplicationDbContextExtensions
    {
        public static void MigrateAndSeedData(this ApplicationDbContext context)
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
                var userToInsert = new ApplicationUser { Email = "test@test.com", UserName = "Test Testerson", PhoneNumber = "12345678" };
                var password = new PasswordHasher<ApplicationUser>();
                var hashed = password.HashPassword(userToInsert, "Password@123");
                userToInsert.PasswordHash = hashed;

                var userStore = new UserStore<ApplicationUser>(context);
                userStore.CreateAsync(userToInsert).Wait();
            }
#endif
        }
    }
}
