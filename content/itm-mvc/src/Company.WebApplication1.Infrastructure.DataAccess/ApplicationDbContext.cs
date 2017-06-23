using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Company.WebApplication1.Core.Entities;
using System;

namespace Company.WebApplication1.Infrastructure.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
#if (Examples)
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set;}
        public DbSet<Student> Students { get; set; }

#endif
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        public void CommitWithIdentityInsert(string tableName)
        {
            using (var dbContextTransaction = Database.BeginTransaction())
            {
                try
                {
                    Database.ExecuteSqlCommand($@"SET IDENTITY_INSERT {tableName} ON");
                    SaveChanges();
                    Database.ExecuteSqlCommand($@"SET IDENTITY_INSERT {tableName} OFF");
                    dbContextTransaction.Commit();

                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    throw;
                }
            }
        }
    }
}
