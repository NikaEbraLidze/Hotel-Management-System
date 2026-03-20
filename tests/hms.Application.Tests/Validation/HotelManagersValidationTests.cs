using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Validation;

namespace hms.Application.Tests.Validation;

public class HotelManagersValidationTests
{
    [Fact]
    public void ValidateAddHotelManagerRequest_Throws_WhenManagerIdIsEmpty()
    {
        var request = new AddHotelManagerRequestDTO
        {
            ManagerId = Guid.Empty
        };

        var exception = Assert.Throws<BadRequestException>(() => HotelManagersValidation.ValidateAddHotelManagerRequest(Guid.NewGuid(), request));

        Assert.Equal("Manager ID must be provided.", exception.Message);
    }

    [Fact]
    public void ValidateUpdateHotelManagerRequest_Throws_WhenPhoneNumberIsInvalid()
    {
        var request = new UpdateHotelManagerRequestDTO
        {
            PhoneNumber = "123-456"
        };

        var exception = Assert.Throws<BadRequestException>(() => HotelManagersValidation.ValidateUpdateHotelManagerRequest(Guid.NewGuid(), Guid.NewGuid(), request));

        Assert.Equal("Phone number format is invalid.", exception.Message);
    }

    [Fact]
    public void ValidateUpdateHotelManagerRequest_DoesNotThrow_WhenRequestIsValid()
    {
        var request = new UpdateHotelManagerRequestDTO
        {
            LastName = "Manager",
            Email = "manager@example.com"
        };

        HotelManagersValidation.ValidateUpdateHotelManagerRequest(Guid.NewGuid(), Guid.NewGuid(), request);
    }
}
