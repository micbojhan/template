using System.Threading.Tasks;

namespace Company.WebApplication1.Core.DomainServices
{
    public interface IUnitOfWork
    {
        int Save();
        Task<int> SaveAsync();
    }
}