using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Company.WebApplication1.Core.Entities;
using Company.WebApplication1.Infrastructure.DataAccess;
using Company.WebApplication1.Core.DomainServices;

namespace Company.WebApplication1.Core.Query
{
    public class QueryDb<T>
    {
        protected readonly IGenericRepository<T> _repo;

        public QueryDb(IGenericRepository<T> repo)
        {
            _repo = repo;
        }
    }
}
