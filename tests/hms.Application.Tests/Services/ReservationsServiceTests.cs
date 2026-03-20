using System.Linq.Expressions;
using hms.Application.Contracts.Repository;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Services;
using hms.Application.Tests.Common;
using hms.Domain.Entities;
using hms.Domain.Identity;
using MapsterMapper;
using Moq;

namespace hms.Application.Tests.Services;

public class ReservationsServiceTests
{
    private readonly Mock<IReservationsRepository> _reservationsRepository = new();
    private readonly Mock<IHotelRepository> _hotelRepository = new();
    private readonly Mock<IHotelManagersRepository> _hotelManagersRepository = new();
    private readonly Mock<IGuestsRepository> _guestsRepository = new();
    private readonly IMapper _mapper = MapsterTestMapperFactory.Create();

    [Fact]
    public async Task CreateReservationAsync_CreatesReservation_WhenRoomsAreAvailable()
    {
        var hotelId = Guid.NewGuid();
        var guest = TestDataFactory.CreateUser(id: Guid.NewGuid(), email: "guest@example.com");
        var room1 = TestDataFactory.CreateRoom(hotelId: hotelId, name: "101", price: 100m);
        var room2 = TestDataFactory.CreateRoom(hotelId: hotelId, name: "102", price: 120m);
        var request = new CreateReservationRequestDTO
        {
            CheckInDate = DateTime.UtcNow.Date.AddDays(3),
            CheckOutDate = DateTime.UtcNow.Date.AddDays(5),
            RoomIds = new List<Guid> { room1.Id, room2.Id }
        };
        var createdReservationId = Guid.NewGuid();
        Reservation capturedReservation = null;

        SetupHotelExists(true);
        _guestsRepository.Setup(repository => repository.GetByIdAsync(guest.Id))
            .ReturnsAsync(guest);
        _reservationsRepository.Setup(repository => repository.GetHotelRoomsByIdsAsync(
                hotelId,
                It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(new[] { room1.Id, room2.Id })),
                false))
            .ReturnsAsync(new List<Room> { room1, room2 });
        _reservationsRepository.Setup(repository => repository.GetUnavailableRoomIdsAsync(
                hotelId,
                It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(new[] { room1.Id, room2.Id })),
                request.CheckInDate.Date,
                request.CheckOutDate.Date,
                null))
            .ReturnsAsync(new List<Guid>());
        _reservationsRepository.Setup(repository => repository.AddAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(reservation =>
            {
                capturedReservation = reservation;
                reservation.Id = createdReservationId;
            })
            .Returns(Task.CompletedTask);
        _reservationsRepository.Setup(repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _reservationsRepository.Setup(repository => repository.GetByIdWithDetailsAsync(createdReservationId, false))
            .ReturnsAsync(TestDataFactory.CreateReservation(
                id: createdReservationId,
                guestId: guest.Id,
                checkInDate: request.CheckInDate.Date,
                checkOutDate: request.CheckOutDate.Date,
                guest: guest,
                rooms: new[] { room1, room2 }));

        var service = CreateService();
        var result = await service.CreateReservationAsync(hotelId, guest.Id, request);

        Assert.NotNull(capturedReservation);
        Assert.Equal(2, capturedReservation.ReservationRooms.Count);
        Assert.Equal(2, capturedReservation.ReservationRooms.Select(room => room.RoomId).Distinct().Count());
        Assert.Equal(createdReservationId, result.Id);
        Assert.Equal(hotelId, result.HotelId);
        Assert.Equal(2, result.Rooms.Count);
    }

    [Fact]
    public async Task CreateReservationAsync_ThrowsUnauthorized_WhenGuestDoesNotExist()
    {
        var hotelId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        SetupHotelExists(true);
        _guestsRepository.Setup(repository => repository.GetByIdAsync(guestId))
            .ReturnsAsync((ApplicationUser)null);

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateReservationAsync(hotelId, guestId, new CreateReservationRequestDTO
        {
            CheckInDate = DateTime.UtcNow.Date.AddDays(2),
            CheckOutDate = DateTime.UtcNow.Date.AddDays(4),
            RoomIds = new List<Guid> { Guid.NewGuid() }
        }));

        Assert.Equal("Authenticated guest account was not found.", exception.Message);
    }

    [Fact]
    public async Task CreateReservationAsync_ThrowsConflict_WhenRequestedRoomsAreUnavailable()
    {
        var hotelId = Guid.NewGuid();
        var guest = TestDataFactory.CreateUser(id: Guid.NewGuid());
        var room = TestDataFactory.CreateRoom(hotelId: hotelId);
        var request = new CreateReservationRequestDTO
        {
            CheckInDate = DateTime.UtcNow.Date.AddDays(3),
            CheckOutDate = DateTime.UtcNow.Date.AddDays(5),
            RoomIds = new List<Guid> { room.Id }
        };

        SetupHotelExists(true);
        _guestsRepository.Setup(repository => repository.GetByIdAsync(guest.Id))
            .ReturnsAsync(guest);
        _reservationsRepository.Setup(repository => repository.GetHotelRoomsByIdsAsync(hotelId, It.IsAny<IEnumerable<Guid>>(), false))
            .ReturnsAsync(new List<Room> { room });
        _reservationsRepository.Setup(repository => repository.GetUnavailableRoomIdsAsync(
                hotelId,
                It.IsAny<IEnumerable<Guid>>(),
                request.CheckInDate.Date,
                request.CheckOutDate.Date,
                null))
            .ReturnsAsync(new List<Guid> { room.Id });

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.CreateReservationAsync(hotelId, guest.Id, request));

        Assert.Equal($"Rooms with IDs {room.Id} are not available for the selected date range.", exception.Message);
    }

    [Fact]
    public async Task GetReservationByIdAsync_ThrowsForbidden_WhenReservationBelongsToDifferentGuest()
    {
        var reservationId = Guid.NewGuid();
        var currentGuest = TestDataFactory.CreateUser(id: Guid.NewGuid());
        var otherGuest = TestDataFactory.CreateUser(id: Guid.NewGuid(), email: "other@example.com");
        var room = TestDataFactory.CreateRoom(hotelId: Guid.NewGuid());
        var reservation = TestDataFactory.CreateReservation(
            id: reservationId,
            guestId: otherGuest.Id,
            guest: otherGuest,
            rooms: new[] { room });

        _guestsRepository.Setup(repository => repository.GetByIdAsync(currentGuest.Id))
            .ReturnsAsync(currentGuest);
        _reservationsRepository.Setup(repository => repository.GetByIdWithDetailsAsync(reservationId, false))
            .ReturnsAsync(reservation);

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<ForbiddenException>(() => service.GetReservationByIdAsync(reservationId, currentGuest.Id));

        Assert.Equal("You do not have access to this reservation.", exception.Message);
    }

    [Fact]
    public async Task DeleteReservationAsync_ThrowsConflict_WhenReservationStartsToday()
    {
        var guest = TestDataFactory.CreateUser(id: Guid.NewGuid());
        var reservationId = Guid.NewGuid();
        var room = TestDataFactory.CreateRoom(hotelId: Guid.NewGuid());
        var reservation = TestDataFactory.CreateReservation(
            id: reservationId,
            guestId: guest.Id,
            checkInDate: DateTime.UtcNow.Date,
            checkOutDate: DateTime.UtcNow.Date.AddDays(2),
            guest: guest,
            rooms: new[] { room });

        _guestsRepository.Setup(repository => repository.GetByIdAsync(guest.Id))
            .ReturnsAsync(guest);
        _reservationsRepository.Setup(repository => repository.GetByIdWithDetailsAsync(reservationId, true))
            .ReturnsAsync(reservation);

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.DeleteReservationAsync(reservationId, guest.Id));

        Assert.Equal("Only future reservations can be updated or cancelled.", exception.Message);
    }

    [Fact]
    public async Task GetHotelReservationsAsync_ThrowsForbidden_WhenManagerHasNoAccess()
    {
        var hotelId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        SetupHotelExists(true);
        _hotelManagersRepository.Setup(repository => repository.ExistsAsync(hotelId, managerId))
            .ReturnsAsync(false);

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<ForbiddenException>(() => service.GetHotelReservationsAsync(
            hotelId,
            managerId,
            isAdmin: false,
            request: new GetHotelReservationsRequestDTO()));

        Assert.Equal($"You do not have access to hotel {hotelId} reservations.", exception.Message);
    }

    private ReservationsService CreateService()
        => new(_reservationsRepository.Object, _hotelRepository.Object, _hotelManagersRepository.Object, _guestsRepository.Object, _mapper);

    private void SetupHotelExists(bool exists)
    {
        _hotelRepository.Setup(repository => repository.ExistsAsync(It.IsAny<Expression<Func<Hotel, bool>>>() ))
            .ReturnsAsync(exists);
    }
}

