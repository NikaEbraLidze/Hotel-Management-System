using hms.Application.Contracts.Repository;
using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Validation;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;

namespace hms.Application.Services
{
    public class GuestsService : IGuestsService
    {
        private readonly IGuestsRepository _guestsRepository;
        private readonly IMapper _mapper;

        public GuestsService(IGuestsRepository guestsRepository, IMapper mapper)
        {
            _guestsRepository = guestsRepository;
            _mapper = mapper;
        }

        public async Task<List<GetGuestsResponseDTO>> GetGuestsAsync()
        {
            var guests = await _guestsRepository.GetAllAsync();
            return _mapper.Map<List<GetGuestsResponseDTO>>(guests);
        }

        public async Task<GetGuestByIdResponseDTO> GetGuestByIdAsync(Guid guestId)
        {
            GuestValidation.ValidateGuestId(guestId);

            var guest = await _guestsRepository.GetByIdAsync(guestId)
                ?? throw new NotFoundException($"Guest with ID {guestId} not found.");

            return _mapper.Map<GetGuestByIdResponseDTO>(guest);
        }

        public async Task<UpdateGuestResponseDTO> UpdateGuestAsync(Guid guestId, UpdateGuestRequestDTO request)
        {
            GuestValidation.ValidateUpdateGuestRequest(guestId, request);

            var guest = await _guestsRepository.GetByIdAsync(guestId)
                ?? throw new NotFoundException($"Guest with ID {guestId} not found.");

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var normalizedEmail = request.Email.Trim();

                if (!string.Equals(guest.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
                    await _guestsRepository.UserExistsByEmailAsync(normalizedEmail))
                {
                    throw new ConflictException($"User with email {normalizedEmail} already exists.");
                }
            }

            _mapper.Map(request, guest);

            var result = await _guestsRepository.UpdateAsync(guest);
            ThrowIfIdentityOperationFailed(result, "Guest update failed");

            return _mapper.Map<UpdateGuestResponseDTO>(guest);
        }

        public async Task DeleteGuestAsync(Guid guestId)
        {
            GuestValidation.ValidateGuestId(guestId);

            var guest = await _guestsRepository.GetByIdAsync(guestId)
                ?? throw new NotFoundException($"Guest with ID {guestId} not found.");

            var hasReservations = await _guestsRepository.HasReservationsAsync(guestId);

            if (hasReservations)
                throw new ConflictException(
                    $"Guest with ID {guestId} cannot be deleted because it has reservations.");

            var result = await _guestsRepository.DeleteAsync(guest);
            ThrowIfIdentityOperationFailed(result, "Guest deletion failed");
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
