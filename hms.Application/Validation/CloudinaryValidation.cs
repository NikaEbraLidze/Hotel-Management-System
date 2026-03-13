using hms.Application.Models.Exceptions;
using Microsoft.AspNetCore.Http;

namespace hms.Application.Validation
{
    public static class CloudinaryValidation
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        public static void ValidateUploadFile(IFormFile file)
        {
            if (file is null)
                throw new BadRequestException("Image file is required.");

            if (file.Length <= 0)
                throw new BadRequestException("Image file must not be empty.");

            if (string.IsNullOrWhiteSpace(file.FileName))
                throw new BadRequestException("Image file name is required.");

            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                throw new BadRequestException("Only .jpg, .jpeg, .png, and .webp image files are supported.");

            if (!string.IsNullOrWhiteSpace(file.ContentType) &&
                !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Only image uploads are supported.");
        }

        public static void ValidatePublicId(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new BadRequestException("Public ID is required.");
        }
    }
}
