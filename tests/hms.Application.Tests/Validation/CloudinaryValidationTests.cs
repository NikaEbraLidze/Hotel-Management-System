using hms.Application.Models.Exceptions;
using hms.Application.Validation;
using hms.Application.Tests.Common;

namespace hms.Application.Tests.Validation;

public class CloudinaryValidationTests
{
    [Fact]
    public void ValidateUploadFile_Throws_WhenFileIsNull()
    {
        var exception = Assert.Throws<BadRequestException>(() => CloudinaryValidation.ValidateUploadFile(null));

        Assert.Equal("Image file is required.", exception.Message);
    }

    [Fact]
    public void ValidateUploadFile_Throws_WhenExtensionIsNotSupported()
    {
        var file = FormFileFactory.Create(fileName: "image.txt", contentType: "image/plain");

        var exception = Assert.Throws<BadRequestException>(() => CloudinaryValidation.ValidateUploadFile(file));

        Assert.Equal("Only .jpg, .jpeg, .png, and .webp image files are supported.", exception.Message);
    }

    [Fact]
    public void ValidatePublicId_Throws_WhenPublicIdIsMissing()
    {
        var exception = Assert.Throws<BadRequestException>(() => CloudinaryValidation.ValidatePublicId(" "));

        Assert.Equal("Public ID is required.", exception.Message);
    }

    [Fact]
    public void ValidateUploadFile_DoesNotThrow_WhenFileIsValid()
    {
        var file = FormFileFactory.Create(fileName: "image.webp", contentType: "image/webp");

        CloudinaryValidation.ValidateUploadFile(file);
    }
}
