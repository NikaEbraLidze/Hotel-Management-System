using System.Linq.Expressions;

namespace hms.Application.Contracts.Repository
{
    public interface IRepositoryBase<T> where T : class
    {
        Task AddAsync(T entity);
        void UpdateAsync(T entity);
        void DeleteAsync(T entity);
        void DeleteRangeAsync(IEnumerable<T> entities);
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetAsync(
            Expression<Func<T, bool>> filter,
            Func<IQueryable<T>, IQueryable<T>> includes = null,
            bool tracking = true);
        Task<(List<T> Items, int TotalCount)> GetAllAsync(
            Expression<Func<T, bool>> filter = null,
            int? pageNumber = null,
            int? pageSize = null,
            string orderBy = null,
            bool ascending = true,
            Func<IQueryable<T>, IQueryable<T>> includes = null,
            bool tracking = true);
    }
}