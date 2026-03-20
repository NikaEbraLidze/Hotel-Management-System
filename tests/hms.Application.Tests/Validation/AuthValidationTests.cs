using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Validation;

namespace hms.Application.Tests.Validation;

public class AuthValidationTests
{
    [Fact]
    public void ValidateRegistrationRequest_Throws_WhenRequestIsNull()
    {
        var exception = Assert.Throws<BadRequestException>(() => AuthValidation.ValidateRegistrationRequest(null));

        Assert.Equal("Request body is required.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateLoginRequest_Throws_WhenEmailIsMissing(string email)
    {
        var request = new LoginRequestDTO
        {
            Email = email,
            Password = "Password123"
        };

        var exception = Assert.Throws<BadRequestException>(() => AuthValidation.ValidateLoginRequest(request));

        Assert.Equal("Email is required.", exception.Message);
    }

    [Fact]
    public void ValidateRegistrationRequest_DoesNotThrow_WhenRequestIsValid()
    {
        var request = new RegistrationRequestDTO
        {
            FirstName = "Nika",
            LastName = "Ebralidze",
            Email = "nika@example.com",
            Password = "Password123",
            PersonalNumber = "12345678901",
            PhoneNumber = "+995599123456"
        };

        AuthValidation.ValidateRegistrationRequest(request);
    }
}
