using Company.WebApplication1.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Company.WebApplication1.Application.MVC.Tests.Helpers
{
    public class InMemoryApplicationDbContext : ApplicationDbContext
    {
        public InMemoryApplicationDbContext(string databaseName)
            : base(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName).Options)
        {
        }
    }
}
