using hms.Application.Contracts.Repository;
using hms.Domain.Entities;
using hms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace hms.Infrastructure.Repository
{
    public class HotelManagersRepository : RepositoryBase<HotelManager>, IHotelManagersRepository
    {
        public HotelManagersRepository(HmsDbContext context) : base(context)
        {
        }

        public Task<int> CountByHotelIdAsync(Guid hotelId)
        {
            return _dbSet.CountAsync(hotelManager => hotelManager.HotelId == hotelId);
        }

        public async Task<List<HotelManager>> GetByHotelIdAsync(Guid hotelId, bool tracking = false)
        {
            IQueryable<HotelManager> query = _dbSet
                .Include(hotelManager => hotelManager.ManagerUser)
                .Where(hotelManager => hotelManager.HotelId == hotelId);

            if (!tracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

        public async Task<HotelManager> GetByHotelAndManagerIdAsync(Guid hotelId, Guid managerId, bool tracking = false)
        {
            IQueryable<HotelManager> query = _dbSet
                .Include(hotelManager => hotelManager.ManagerUser)
                .Where(hotelManager => hotelManager.HotelId == hotelId && hotelManager.ManagerUserId == managerId);

            if (!tracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync();
        }

        public Task<bool> ExistsAsync(Guid hotelId, Guid managerId)
        {
            return _dbSet.AnyAsync(hotelManager => hotelManager.HotelId == hotelId && hotelManager.ManagerUserId == managerId);
        }
    }
}
