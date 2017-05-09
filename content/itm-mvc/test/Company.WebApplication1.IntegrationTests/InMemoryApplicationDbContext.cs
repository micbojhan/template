using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.PlatformAbstractions;
using Company.WebApplication1.Core.Entities;
using Company.WebApplication1.Infrastructure.DataAccess;
using Company.WebApplication1.Infrastructure.DataAccess.CsvSeeder;

namespace Company.WebApplication1.UnitTests
{
    public class InMemoryApplicationDbContext : ApplicationDbContext
    {
        public InMemoryApplicationDbContext(string databaseName) : base(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName).Options)
        {
        }

        public void Seed()
        {
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
            string seedDataDirectory = Path.Combine(applicationBasePath, "../../../../../src/Company.WebApplication1.Application.MVC/SeedData/");

            Users.SeedFromFile(seedDataDirectory + "/contacts.csv");
        }
    }
}
