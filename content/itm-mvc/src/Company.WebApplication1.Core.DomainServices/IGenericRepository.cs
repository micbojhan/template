using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Company.WebApplication1.Core.DomainServices
{
    public interface IGenericRepository<T>
    {
        IEnumerable<T> Get(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int? page = null,
            int? pageSize = null);
        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "",
            int? page = null,
            int? pageSize = null);
        IQueryable<T> AsQueryable();
        T GetByKey(params object[] key);
        Task<T> GetByKeyAsync(params object[] key);
        T Insert(T entity);
        void DeleteByKey(params object[] key);
        void Update(T entity);
        int Count(Expression<Func<T, bool>> filter = null);
        Task<int> CountAsync(Expression<Func<T, bool>> filter = null);
    }
}