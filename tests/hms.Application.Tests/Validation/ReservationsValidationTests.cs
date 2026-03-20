using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Validation;

namespace hms.Application.Tests.Validation;

public class ReservationsValidationTests
{
    [Fact]
    public void ValidateCreateReservationRequest_Throws_WhenRoomIdsContainDuplicates()
    {
        var roomId = Guid.NewGuid();
        var request = new CreateReservationRequestDTO
        {
            CheckInDate = DateTime.UtcNow.Date.AddDays(1),
            CheckOutDate = DateTime.UtcNow.Date.AddDays(2),
            RoomIds = new List<Guid> { roomId, roomId }
        };

        var exception = Assert.Throws<BadRequestException>(() => ReservationsValidation.ValidateCreateReservationRequest(Guid.NewGuid(), request));

        Assert.Equal("Duplicate room IDs are not allowed.", exception.Message);
    }

    [Fact]
    public void ValidateCreateReservationRequest_Throws_WhenCheckInDateIsInThePast()
    {
        var request = new CreateReservationRequestDTO
        {
            CheckInDate = DateTime.UtcNow.Date.AddDays(-1),
            CheckOutDate = DateTime.UtcNow.Date.AddDays(1),
            RoomIds = new List<Guid> { Guid.NewGuid() }
        };

        var exception = Assert.Throws<BadRequestException>(() => ReservationsValidation.ValidateCreateReservationRequest(Guid.NewGuid(), request));

        Assert.Equal("Check-in date must be today or later.", exception.Message);
    }

    [Fact]
    public void ValidateGetHotelReservationsRequest_DoesNotThrow_WhenRequestIsValid()
    {
        var request = new GetHotelReservationsRequestDTO
        {
            CheckInFrom = DateTime.UtcNow.Date,
            CheckInTo = DateTime.UtcNow.Date.AddDays(7),
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "CheckOutDate",
            Ascending = false
        };

        ReservationsValidation.ValidateGetHotelReservationsRequest(Guid.NewGuid(), request);
    }
}
