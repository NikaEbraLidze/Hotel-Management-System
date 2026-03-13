using System.Linq.Expressions;
using hms.Application.Contracts.Repository;
using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Validation;
using hms.Domain.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace hms.Application.Services
{
    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        public HotelService(IHotelRepository hotelRepository, IMapper mapper, ICloudinaryService cloudinaryService)
        {
            _hotelRepository = hotelRepository;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
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
                    Country = h.Country,
                    ImageUrl = h.ImageUrl,
                    ImagePublicId = h.ImgPublicId
                }).ToList(),
                TotalCount = result.TotalCount,
                PageNumber = request.PageNumber ?? 1,
                PageSize = request.PageSize ?? 10
            };

            return response;
        }

        public async Task<GetHotelByIdResponseDTO> GetHotelByIdAsync(Guid request)
        {
            HotelValidation.ValidateGuid(request);

            var hotel = await _hotelRepository.GetAsync(h => h.Id == request)
                ?? throw new NotFoundException($"Hotel with ID {request} not found.");

            return _mapper.Map<GetHotelByIdResponseDTO>(hotel);
        }

        public async Task<RegisterHotelResponseDTO> RegisterHotelAsync(RegisterHotelRequestDTO request, IFormFile image)
        {
            HotelValidation.ValidateRegisterHotelRequest(request);

            var hotel = _mapper.Map<Hotel>(request);

            await ApplyHotelImageAsync(hotel, image);

            await _hotelRepository.AddAsync(hotel);
            await _hotelRepository.SaveAsync();
            var createdHotel = await _hotelRepository.GetAsync(h => h.Name == hotel.Name && h.Address == hotel.Address)
                ?? throw new NotFoundException("Failed to retrieve the created hotel.");

            return _mapper.Map<RegisterHotelResponseDTO>(createdHotel);
        }

        public async Task<UpdateHotelResponseDTO> UpdateHotelAsync(Guid id, UpdateHotelRequestDTO request, IFormFile image)
        {
            HotelValidation.ValidateUpdateHotelRequest(id, request);

            var hotel = await _hotelRepository.GetAsync(h => h.Id == id)
                ?? throw new NotFoundException($"Hotel with ID {id} not found.");

            hotel.Name = request.Name ?? hotel.Name;
            hotel.Rating = request.Rating ?? hotel.Rating;
            hotel.Address = request.Address ?? hotel.Address;
            hotel.City = request.City ?? hotel.City;
            hotel.Country = request.Country ?? hotel.Country;
            await ApplyHotelImageAsync(hotel, image);

            _hotelRepository.UpdateAsync(hotel);
            await _hotelRepository.SaveAsync();

            return _mapper.Map<UpdateHotelResponseDTO>(hotel);
        }

        public async Task DeleteHotelAsync(Guid id)
        {
            HotelValidation.ValidateGuid(id);

            var hotel = await _hotelRepository.GetAsync(h => h.Id == id)
                ?? throw new NotFoundException($"Hotel with ID {id} not found.");

            _hotelRepository.DeleteAsync(hotel);
            await _hotelRepository.SaveAsync();
        }

        private async Task ApplyHotelImageAsync(Hotel hotel, IFormFile image)
        {
            if (image == null)
                return;

            var uploadResult = string.IsNullOrWhiteSpace(hotel.ImgPublicId)
                ? await _cloudinaryService.UploadImageAsync(image)
                : await _cloudinaryService.UpdateImageAsync(hotel.ImgPublicId, image);

            hotel.ImageUrl = uploadResult.Url;
            hotel.ImgPublicId = uploadResult.PublicId;
        }
    }
}
