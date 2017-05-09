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
