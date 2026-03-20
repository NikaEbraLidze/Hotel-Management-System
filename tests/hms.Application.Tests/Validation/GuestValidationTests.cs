using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Validation;

namespace hms.Application.Tests.Validation;

public class GuestValidationTests
{
    [Fact]
    public void ValidateUpdateGuestRequest_Throws_WhenNoFieldsAreProvided()
    {
        var request = new UpdateGuestRequestDTO();

        var exception = Assert.Throws<BadRequestException>(() => GuestValidation.ValidateUpdateGuestRequest(Guid.NewGuid(), request));

        Assert.Equal("At least one field must be provided for update.", exception.Message);
    }

    [Fact]
    public void ValidateUpdateGuestRequest_Throws_WhenEmailIsInvalid()
    {
        var request = new UpdateGuestRequestDTO
        {
            Email = "not-an-email"
        };

        var exception = Assert.Throws<BadRequestException>(() => GuestValidation.ValidateUpdateGuestRequest(Guid.NewGuid(), request));

        Assert.Equal("Email format is invalid.", exception.Message);
    }

    [Fact]
    public void ValidateUpdateGuestRequest_DoesNotThrow_WhenRequestIsValid()
    {
        var request = new UpdateGuestRequestDTO
        {
            FirstName = "Nika",
            Email = "nika@example.com"
        };

        GuestValidation.ValidateUpdateGuestRequest(Guid.NewGuid(), request);
    }
}
