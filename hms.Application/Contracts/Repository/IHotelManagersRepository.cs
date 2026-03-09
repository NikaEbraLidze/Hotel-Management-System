using hms.Domain.Entities;

namespace hms.Application.Contracts.Repository
{
    public interface IHotelManagersRepository : IRepositoryBase<HotelManager>
    {
        Task<int> CountByHotelIdAsync(Guid hotelId);
        Task<List<HotelManager>> GetByHotelIdAsync(Guid hotelId, bool tracking = false);
        Task<HotelManager> GetByHotelAndManagerIdAsync(Guid hotelId, Guid managerId, bool tracking = false);
        Task<bool> ExistsAsync(Guid hotelId, Guid managerId);
    }
}
