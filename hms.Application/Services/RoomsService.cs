using System.Linq.Expressions;
using hms.Application.Contracts.Repository;
using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Validation;
using hms.Domain.Entities;
using MapsterMapper;

namespace hms.Application.Services
{
    public class RoomsService : IRoomsService
    {
        private readonly IRoomsRepository _roomsRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;

        public RoomsService(IRoomsRepository roomsRepository, IHotelRepository hotelRepository, IMapper mapper)
        {
            _roomsRepository = roomsRepository;
            _hotelRepository = hotelRepository;
            _mapper = mapper;
        }

        public async Task<PagedResponseDTO<GetRoomsResponseDTO>> GetRoomsAsync(Guid hotelId, GetRoomsRequestDTO request)
        {
            RoomsValidation.ValidateGetRoomsRequest(hotelId, request);
            await EnsureHotelExistsAsync(hotelId);

            var normalizedName = Normalize(request.Name);
            var normalizedOrderBy = Normalize(request.OrderBy);

            Expression<Func<Room, bool>> filter = room =>
                room.HotelId == hotelId &&
                (string.IsNullOrEmpty(normalizedName) || room.Name.Contains(normalizedName)) &&
                (!request.Price.HasValue || room.Price == request.Price.Value);

            var rooms = await _roomsRepository.GetAllAsync(
                filter: filter,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                orderBy: normalizedOrderBy,
                ascending: request.Ascending,
                tracking: false);

            return new PagedResponseDTO<GetRoomsResponseDTO>
            {
                Items = _mapper.Map<List<GetRoomsResponseDTO>>(rooms.Items),
                TotalCount = rooms.TotalCount,
                PageNumber = request.PageNumber ?? 1,
                PageSize = request.PageSize ?? rooms.Items.Count
            };
        }

        public async Task<GetRoomsResponseDTO> GetRoomByIdAsync(Guid hotelId, Guid roomId)
        {
            RoomsValidation.ValidateRoomRoute(hotelId, roomId);
            await EnsureHotelExistsAsync(hotelId);

            var room = await GetRoomEntityAsync(hotelId, roomId, tracking: false);
            return _mapper.Map<GetRoomsResponseDTO>(room);
        }

        public async Task<GetRoomsResponseDTO> CreateRoomAsync(Guid hotelId, CreateRoomRequestDTO request)
        {
            RoomsValidation.ValidateCreateRoomRequest(hotelId, request);
            await EnsureHotelExistsAsync(hotelId);

            var room = _mapper.Map<Room>(request);
            room.HotelId = hotelId;

            await _roomsRepository.AddAsync(room);
            await _roomsRepository.SaveAsync();

            return _mapper.Map<GetRoomsResponseDTO>(room);
        }

        public async Task<GetRoomsResponseDTO> UpdateRoomAsync(Guid hotelId, Guid roomId, UpdateRoomRequestDTO request)
        {
            RoomsValidation.ValidateUpdateRoomRequest(hotelId, roomId, request);
            await EnsureHotelExistsAsync(hotelId);

            var room = await GetRoomEntityAsync(hotelId, roomId);

            _mapper.Map<UpdateRoomRequestDTO, Room>(request, room);

            _roomsRepository.UpdateAsync(room);
            await _roomsRepository.SaveAsync();

            return _mapper.Map<GetRoomsResponseDTO>(room);
        }

        public async Task DeleteRoomAsync(Guid hotelId, Guid roomId)
        {
            RoomsValidation.ValidateRoomRoute(hotelId, roomId);
            await EnsureHotelExistsAsync(hotelId);

            var room = await GetRoomEntityAsync(hotelId, roomId);
            var hasActiveOrFutureReservations = await _roomsRepository.HasActiveOrFutureReservationsAsync(room.Id, DateTime.UtcNow);

            if (hasActiveOrFutureReservations)
                throw new ConflictException(
                    $"Room with ID {roomId} cannot be deleted because it has active or future reservations.");

            _roomsRepository.DeleteAsync(room);
            await _roomsRepository.SaveAsync();
        }

        private async Task EnsureHotelExistsAsync(Guid hotelId)
        {
            var hotelExists = await _hotelRepository.ExistsAsync(hotel => hotel.Id == hotelId);

            if (!hotelExists)
                throw new NotFoundException($"Hotel with ID {hotelId} not found.");
        }

        private async Task<Room> GetRoomEntityAsync(Guid hotelId, Guid roomId, bool tracking = true)
        {
            return await _roomsRepository.GetAsync(
                filter: room => room.HotelId == hotelId && room.Id == roomId,
                tracking: tracking)
                ?? throw new NotFoundException($"Room with ID {roomId} not found for hotel {hotelId}.");
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
