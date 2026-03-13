using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Validation;
using Microsoft.AspNetCore.Http;

namespace hms.Application.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<CloudinaryImageResponseDTO> UploadImageAsync(IFormFile file)
        {
            CloudinaryValidation.ValidateUploadFile(file);

            return await UploadToCloudinaryAsync(file);
        }

        public async Task<CloudinaryImageResponseDTO> UpdateImageAsync(string publicId, IFormFile file)
        {
            CloudinaryValidation.ValidatePublicId(publicId);
            CloudinaryValidation.ValidateUploadFile(file);

            return await UploadToCloudinaryAsync(file, publicId.Trim());
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            CloudinaryValidation.ValidatePublicId(publicId);

            return await DeleteFromCloudinaryAsync(publicId.Trim());
        }

        private async Task<CloudinaryImageResponseDTO> UploadToCloudinaryAsync(IFormFile file, string publicId = null)
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "hms",
                PublicId = publicId,
                Overwrite = !string.IsNullOrWhiteSpace(publicId),
                Invalidate = !string.IsNullOrWhiteSpace(publicId)
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error is not null)
                throw new BadRequestException($"Cloudinary upload failed: {uploadResult.Error.Message}");

            if (uploadResult.SecureUrl is null || string.IsNullOrWhiteSpace(uploadResult.PublicId))
                throw new BadRequestException("Cloudinary upload did not return a valid image result.");

            return new CloudinaryImageResponseDTO
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.SecureUrl.ToString()
            };
        }

        private async Task<bool> DeleteFromCloudinaryAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            if (deletionResult.Error is not null)
                throw new BadRequestException($"Cloudinary delete failed: {deletionResult.Error.Message}");

            if (string.Equals(deletionResult.Result, "not found", StringComparison.OrdinalIgnoreCase))
                throw new NotFoundException($"Image with public ID '{publicId}' was not found.");

            return string.Equals(deletionResult.Result, "ok", StringComparison.OrdinalIgnoreCase);
        }
    }
}
