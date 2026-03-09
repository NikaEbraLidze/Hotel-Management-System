using hms.Application.Contracts.Repository;
using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Validation;
using hms.Domain.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;

namespace hms.Application.Services
{
    public class HotelManagersService : IHotelManagersService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IManagersRepository _managersRepository;
        private readonly IHotelManagersRepository _hotelManagersRepository;
        private readonly IMapper _mapper;

        public HotelManagersService(
            IHotelRepository hotelRepository,
            IManagersRepository managersRepository,
            IHotelManagersRepository hotelManagersRepository,
            IMapper mapper)
        {
            _hotelRepository = hotelRepository;
            _managersRepository = managersRepository;
            _hotelManagersRepository = hotelManagersRepository;
            _mapper = mapper;
        }

        public async Task<AddHotelManagerResponseDTO> AddHotelManagerAsync(Guid hotelId, AddHotelManagerRequestDTO request)
        {
            HotelManagersValidation.ValidateAddHotelManagerRequest(hotelId, request);
            await EnsureHotelExistsAsync(hotelId);

            var manager = await _managersRepository.GetByIdAsync(request.ManagerId)
                ?? throw new NotFoundException($"Manager with ID {request.ManagerId} not found.");

            var relationExists = await _hotelManagersRepository.ExistsAsync(hotelId, request.ManagerId);

            if (relationExists)
                throw new ConflictException(
                    $"Manager with ID {request.ManagerId} is already assigned to hotel {hotelId}.");

            var hotelManager = _mapper.Map<HotelManager>(request);
            hotelManager.HotelId = hotelId;
            hotelManager.ManagerUser = manager;

            await _hotelManagersRepository.AddAsync(hotelManager);
            await _hotelManagersRepository.SaveAsync();

            return _mapper.Map<AddHotelManagerResponseDTO>(hotelManager);
        }

        public async Task<List<GetHotelManagersResponseDTO>> GetHotelManagersAsync(Guid hotelId)
        {
            HotelManagersValidation.ValidateGetHotelManagersRequest(hotelId);
            await EnsureHotelExistsAsync(hotelId);

            var hotelManagers = await _hotelManagersRepository.GetByHotelIdAsync(hotelId);
            return _mapper.Map<List<GetHotelManagersResponseDTO>>(hotelManagers);
        }

        public async Task<GetHotelManagerByIdResponseDTO> GetHotelManagerByIdAsync(Guid hotelId, Guid managerId)
        {
            HotelManagersValidation.ValidateManagerRoute(hotelId, managerId);
            await EnsureHotelExistsAsync(hotelId);

            var hotelManager = await _hotelManagersRepository.GetByHotelAndManagerIdAsync(hotelId, managerId)
                ?? throw new NotFoundException($"Manager with ID {managerId} not found for hotel {hotelId}.");

            return _mapper.Map<GetHotelManagerByIdResponseDTO>(hotelManager);
        }

        public async Task<UpdateHotelManagerResponseDTO> UpdateHotelManagerAsync(Guid hotelId, Guid managerId, UpdateHotelManagerRequestDTO request)
        {
            HotelManagersValidation.ValidateUpdateHotelManagerRequest(hotelId, managerId, request);
            await EnsureHotelExistsAsync(hotelId);

            var hotelManager = await _hotelManagersRepository.GetByHotelAndManagerIdAsync(hotelId, managerId, tracking: true)
                ?? throw new NotFoundException($"Manager with ID {managerId} not found for hotel {hotelId}.");

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var normalizedEmail = request.Email.Trim();

                if (!string.Equals(hotelManager.ManagerUser.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
                    await _managersRepository.UserExistsByEmailAsync(normalizedEmail))
                {
                    throw new ConflictException($"User with email {normalizedEmail} already exists.");
                }
            }

            _mapper.Map(request, hotelManager.ManagerUser);

            var result = await _managersRepository.UpdateAsync(hotelManager.ManagerUser);
            ThrowIfIdentityOperationFailed(result, "Manager update failed");

            return _mapper.Map<UpdateHotelManagerResponseDTO>(hotelManager);
        }

        public async Task DeleteHotelManagerAsync(Guid hotelId, Guid managerId)
        {
            HotelManagersValidation.ValidateManagerRoute(hotelId, managerId);
            await EnsureHotelExistsAsync(hotelId);

            var hotelManager = await _hotelManagersRepository.GetByHotelAndManagerIdAsync(hotelId, managerId, tracking: true)
                ?? throw new NotFoundException($"Manager with ID {managerId} not found for hotel {hotelId}.");

            _hotelManagersRepository.DeleteAsync(hotelManager);
            await _hotelManagersRepository.SaveAsync();
        }

        private async Task EnsureHotelExistsAsync(Guid hotelId)
        {
            var hotelExists = await _hotelRepository.ExistsAsync(hotel => hotel.Id == hotelId);

            if (!hotelExists)
                throw new NotFoundException($"Hotel with ID {hotelId} not found.");
        }

        private static void ThrowIfIdentityOperationFailed(IdentityResult result, string errorPrefix)
        {
            if (result.Succeeded)
                return;

            var errors = result.Errors
                .Select(error => $"{errorPrefix}: {error.Description}")
                .ToList();

            throw new IdentityOperationException(errors);
        }
    }
}
