using System.Linq.Expressions;
using hms.Application.Contracts.Repository;
using hms.Application.Models.DTO;
using hms.Application.Models.Exceptions;
using hms.Application.Services;
using hms.Application.Tests.Common;
using hms.Domain.Entities;
using MapsterMapper;
using Moq;

namespace hms.Application.Tests.Services;

public class RoomsServiceTests
{
    private readonly Mock<IRoomsRepository> _roomsRepository = new();
    private readonly Mock<IHotelRepository> _hotelRepository = new();
    private readonly IMapper _mapper = MapsterTestMapperFactory.Create();

    [Fact]
    public async Task CreateRoomAsync_SetsHotelIdAndSaves()
    {
        var hotelId = Guid.NewGuid();
        var request = new CreateRoomRequestDTO
        {
            Name = " Deluxe ",
            Price = 199.99m
        };

        Room capturedRoom = null;
        _hotelRepository.Setup(repository => repository.ExistsAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(true);
        _roomsRepository.Setup(repository => repository.AddAsync(It.IsAny<Room>()))
            .Callback<Room>(room => capturedRoom = room)
            .Returns(Task.CompletedTask);
        _roomsRepository.Setup(repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();
        var result = await service.CreateRoomAsync(hotelId, request);

        Assert.NotNull(capturedRoom);
        Assert.Equal(hotelId, capturedRoom.HotelId);
        Assert.Equal("Deluxe", result.Name);
        Assert.Equal(199.99m, result.Price);
    }

    [Fact]
    public async Task GetRoomsAsync_ThrowsNotFound_WhenHotelDoesNotExist()
    {
        _hotelRepository.Setup(repository => repository.ExistsAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(false);

        var service = CreateService();
        var hotelId = Guid.NewGuid();
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetRoomsAsync(hotelId, new GetRoomsRequestDTO()));

        Assert.Equal($"Hotel with ID {hotelId} not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateRoomAsync_MapsOntoExistingRoom()
    {
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var existingRoom = TestDataFactory.CreateRoom(id: roomId, hotelId: hotelId, name: "Standard", price: 99m);
        var request = new UpdateRoomRequestDTO
        {
            Name = " Premium ",
            Price = 129.50m
        };

        _hotelRepository.Setup(repository => repository.ExistsAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(true);
        _roomsRepository.Setup(repository => repository.GetAsync(
                It.IsAny<Expression<Func<Room, bool>>>(),
                It.IsAny<Func<IQueryable<Room>, IQueryable<Room>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(existingRoom);
        _roomsRepository.Setup(repository => repository.SaveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();
        var result = await service.UpdateRoomAsync(hotelId, roomId, request);

        Assert.Equal("Premium", existingRoom.Name);
        Assert.Equal(129.50m, existingRoom.Price);
        Assert.Equal(roomId, result.Id);
        _roomsRepository.Verify(repository => repository.UpdateAsync(existingRoom), Times.Once);
    }

    [Fact]
    public async Task DeleteRoomAsync_ThrowsConflict_WhenRoomHasActiveOrFutureReservations()
    {
        var hotelId = Guid.NewGuid();
        var room = TestDataFactory.CreateRoom(hotelId: hotelId);

        _hotelRepository.Setup(repository => repository.ExistsAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(true);
        _roomsRepository.Setup(repository => repository.GetAsync(
                It.IsAny<Expression<Func<Room, bool>>>(),
                It.IsAny<Func<IQueryable<Room>, IQueryable<Room>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(room);
        _roomsRepository.Setup(repository => repository.HasActiveOrFutureReservationsAsync(room.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.DeleteRoomAsync(hotelId, room.Id));

        Assert.Equal($"Room with ID {room.Id} cannot be deleted because it has active or future reservations.", exception.Message);
    }

    private RoomsService CreateService() => new(_roomsRepository.Object, _hotelRepository.Object, _mapper);
}
