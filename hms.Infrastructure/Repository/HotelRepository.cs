using hms.Application.Contracts.Repository;
using hms.Domain.Entities;
using hms.Infrastructure.Persistence;

namespace hms.Infrastructure.Repository
{
    public class HotelRepository : RepositoryBase<Hotel>, IHotelRepository
    {
        public HotelRepository(HmsDbContext context) : base(context)
        {
        }
    }
}