using Microsoft.AspNetCore.Http;
using Moq;

namespace hms.Application.Tests.Common;

internal static class FormFileFactory
{
    public static IFormFile Create(
        string fileName = "image.jpg",
        string contentType = "image/jpeg",
        byte[] content = null)
    {
        content ??= new byte[] { 1, 2, 3, 4 };

        var file = new Mock<IFormFile>();
        file.SetupGet(formFile => formFile.FileName).Returns(fileName);
        file.SetupGet(formFile => formFile.ContentType).Returns(contentType);
        file.SetupGet(formFile => formFile.Length).Returns(content.Length);
        file.Setup(formFile => formFile.OpenReadStream()).Returns(new MemoryStream(content));

        return file.Object;
    }
}
