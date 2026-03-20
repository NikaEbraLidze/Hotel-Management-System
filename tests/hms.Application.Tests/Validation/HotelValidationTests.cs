using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Validation;

namespace hms.Application.Tests.Validation;

public class HotelValidationTests
{
    [Fact]
    public void ValidateGuid_Throws_WhenHotelIdIsEmpty()
    {
        var exception = Assert.Throws<BadRequestException>(() => HotelValidation.ValidateGuid(Guid.Empty));

        Assert.Equal("Hotel ID must be provided.", exception.Message);
    }

    [Fact]
    public void ValidateGetHotelsRequest_Throws_WhenOrderByIsInvalid()
    {
        var request = new GetHotelsRequestDTO
        {
            OrderBy = "Stars"
        };

        var exception = Assert.Throws<BadRequestException>(() => HotelValidation.ValidateGetHotelsRequest(request));

        Assert.Contains("Invalid order by field.", exception.Message);
    }

    [Fact]
    public void ValidateRegisterHotelRequest_DoesNotThrow_WhenRequestIsValid()
    {
        var request = new RegisterHotelRequestDTO
        {
            Name = "HMS Hotel",
            Rating = 5,
            Address = "123 Rustaveli Ave",
            City = "Tbilisi",
            Country = "Georgia"
        };

        HotelValidation.ValidateRegisterHotelRequest(request);
    }
}
