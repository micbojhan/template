using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using itmyosample.Core.Entities;
using itmyosample.Infrastructure.DataAccess;

namespace itmyosample.Core.Query
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
