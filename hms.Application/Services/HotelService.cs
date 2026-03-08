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
    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;
        public HotelService(IHotelRepository hotelRepository, IMapper mapper)
        {
            _hotelRepository = hotelRepository;
            _mapper = mapper;
        }
        public async Task<PagedResponseDTO<GetHotelsResponseDTO>> GetHotelsAsync(GetHotelsRequestDTO request)
        {
            HotelValidation.ValidateGetHotelsRequest(request);

            var normalizedName = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim();
            var normalizedCity = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim();
            var normalizedCountry = string.IsNullOrWhiteSpace(request.Country) ? null : request.Country.Trim();
            var normalizedOrderBy = string.IsNullOrWhiteSpace(request.OrderBy) ? null : request.OrderBy.Trim();

            Expression<Func<Hotel, bool>> filter = h =>
                (string.IsNullOrEmpty(normalizedName) || h.Name.Contains(normalizedName)) &&
                (!request.Rating.HasValue || h.Rating == request.Rating) &&
                (string.IsNullOrEmpty(normalizedCity) || h.City.Contains(normalizedCity)) &&
                (string.IsNullOrEmpty(normalizedCountry) || h.Country.Contains(normalizedCountry));

            var result = await _hotelRepository.GetAllAsync(
                filter,
                request.PageNumber,
                request.PageSize,
                normalizedOrderBy,
                request.Ascending
            );

            var response = new PagedResponseDTO<GetHotelsResponseDTO>
            {
                Items = result.Items.Select(h => new GetHotelsResponseDTO
                {
                    Id = h.Id,
                    Name = h.Name,
                    Rating = h.Rating,
                    Address = h.Address,
                    City = h.City,
                    Country = h.Country
                }).ToList(),
                TotalCount = result.TotalCount,
                PageNumber = request.PageNumber ?? 1,
                PageSize = request.PageSize ?? 10
            };

            return response;
        }

        public async Task<GetHotelByIdResponseDTO> GetHotelByIdAsync(Guid request)
        {
            HotelValidation.ValidateGetHotelByIdRequest(request);

            var hotel = await _hotelRepository.GetAsync(h => h.Id == request)
                ?? throw new NotFoundException($"Hotel with ID {request} not found.");

            return _mapper.Map<GetHotelByIdResponseDTO>(hotel);
        }

        public async Task<RegisterHotelResponseDTO> RegisterHotelAsync(RegisterHotelRequestDTO request)
        {
            HotelValidation.ValidateRegisterHotelRequest(request);

            var hotel = _mapper.Map<Hotel>(request);
            await _hotelRepository.AddAsync(hotel);
            await _hotelRepository.SaveAsync();
            var createdHotel = await _hotelRepository.GetAsync(h => h.Name == hotel.Name && h.Address == hotel.Address)
                ?? throw new NotFoundException("Failed to retrieve the created hotel.");

            return _mapper.Map<RegisterHotelResponseDTO>(createdHotel);
        }
    }
}
