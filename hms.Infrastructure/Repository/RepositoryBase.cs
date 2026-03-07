using System.Linq.Expressions;
using System.Reflection;
using hms.Application.Contracts.Repository;
using Microsoft.EntityFrameworkCore;

namespace hms.Infrastructure.Repository
{
    public class RepositoryBase<T, TContext> : IRepositoryBase<T, TContext> where T : class where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly DbSet<T> _dbSet;

        public RepositoryBase(TContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public void UpdateAsync(T entity) => _dbSet.Update(entity);
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) => await _dbSet.AnyAsync(predicate);
        public void DeleteAsync(T entity) => _dbSet.Remove(entity);
        public void DeleteRangeAsync(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);
        public Task<int> SaveAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
        public async Task<T> GetAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IQueryable<T>> includes = null, bool tracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!tracking)
                query = query.AsNoTracking();

            if (includes is not null)
                query = includes(query);

            return await query.FirstOrDefaultAsync(filter);
        }
        public async Task<(List<T> Items, int TotalCount)> GetAllAsync(Expression<Func<T, bool>> filter = null, int? pageNumber = null, int? pageSize = null, string orderBy = null, bool ascending = true, Func<IQueryable<T>, IQueryable<T>> includes = null, bool tracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (!tracking)
                query = query.AsNoTracking();

            if (includes is not null)
                query = includes(query);

            if (filter is not null)
                query = query.Where(filter);

            int totalCount = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(orderBy))
                query = ApplyOrdering(orderBy, ascending, query);

            if (pageNumber.HasValue || pageSize.HasValue)
            {
                if (!pageNumber.HasValue || !pageSize.HasValue)
                    throw new ArgumentException("Both pageNumber and pageSize must be provided.");

                if (pageNumber.Value <= 0)
                    throw new ArgumentException("pageNumber must be greater than 0.");

                if (pageSize.Value <= 0)
                    throw new ArgumentException("pageSize must be greater than 0.");

                int skip = (pageNumber.Value - 1) * pageSize.Value;
                query = query.Skip(skip).Take(pageSize.Value);
            }

            var items = await query.ToListAsync();
            return (items, totalCount);
        }

        private static IQueryable<T> ApplyOrdering(
            string orderBy,
            bool ascending,
            IQueryable<T> query)
        {
            var propertyInfo = typeof(T).GetProperty(
                orderBy,
                BindingFlags.IgnoreCase |
                BindingFlags.Public |
                BindingFlags.Instance);

            if (propertyInfo != null)
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var propertyAccess = Expression.MakeMemberAccess(parameter, propertyInfo);
                var orderByExpression = Expression.Lambda(propertyAccess, parameter);

                var methodName = ascending
                    ? "OrderBy"
                    : "OrderByDescending";

                var resultExpression = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new Type[] { typeof(T), propertyInfo.PropertyType },
                    query.Expression,
                    Expression.Quote(orderByExpression));

                query = query.Provider.CreateQuery<T>(resultExpression);
            }

            return query;
        }


    }
}