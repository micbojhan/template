using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Company.WebApplication1.Core.Entities;
using Company.WebApplication1.Infrastructure.DataAccess;

namespace Company.WebApplication1.Core.Query
{
    public class QueryDb
    {
        private readonly ApplicationDbContext _dbContext;

        public QueryDb(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public IQueryable<ApplicationUser> Users => _dbContext.Users.AsQueryable();
    }
}
