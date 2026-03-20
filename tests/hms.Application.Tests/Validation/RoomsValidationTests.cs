using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Validation;

namespace hms.Application.Tests.Validation;

public class RoomsValidationTests
{
    [Fact]
    public void ValidateGetRoomsRequest_Throws_WhenPriceHasTooManyDecimals()
    {
        var request = new GetRoomsRequestDTO
        {
            Price = 10.123m
        };

        var exception = Assert.Throws<BadRequestException>(() => RoomsValidation.ValidateGetRoomsRequest(Guid.NewGuid(), request));

        Assert.Equal("Price filter must have at most 2 decimal places.", exception.Message);
    }

    [Fact]
    public void ValidateGetRoomsRequest_Throws_WhenPagingIsIncomplete()
    {
        var request = new GetRoomsRequestDTO
        {
            PageNumber = 1
        };

        var exception = Assert.Throws<BadRequestException>(() => RoomsValidation.ValidateGetRoomsRequest(Guid.NewGuid(), request));

        Assert.Equal("Page number and page size must be provided together.", exception.Message);
    }

    [Fact]
    public void ValidateUpdateRoomRequest_DoesNotThrow_WhenRequestIsValid()
    {
        var request = new UpdateRoomRequestDTO
        {
            Name = "Deluxe",
            Price = 199.99m
        };

        RoomsValidation.ValidateUpdateRoomRequest(Guid.NewGuid(), Guid.NewGuid(), request);
    }
}
