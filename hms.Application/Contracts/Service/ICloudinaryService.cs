using hms.Application.Models.DTO;
using Microsoft.AspNetCore.Http;

namespace hms.Application.Contracts.Service
{
    public interface ICloudinaryService
    {
        Task<CloudinaryImageResponseDTO> UploadImageAsync(IFormFile file);
        Task<CloudinaryImageResponseDTO> UpdateImageAsync(string publicId, IFormFile file);
        Task<bool> DeleteImageAsync(string publicId);
    }
}
